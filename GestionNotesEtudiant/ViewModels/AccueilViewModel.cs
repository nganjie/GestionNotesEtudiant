using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Controls;
using GestionNotesEtudiant.Models;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public class UniteItem
{
    public string Code { get; init; } = string.Empty;
    public string Intitule { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public string Statut { get; init; } = string.Empty;
    public VarianteEtiquette VarianteStatut { get; init; }
    public string Moyenne { get; init; } = "—";
}

public class SaisieEnCours
{
    public string Code { get; init; } = string.Empty;
    public string EvaluationId { get; init; } = string.Empty;
    public string Titre { get; init; } = string.Empty;
    public string Progression { get; init; } = string.Empty;
    public double Ratio { get; init; }
}

public partial class AccueilViewModel : VueModeleBase
{
    readonly DonneesService _donnees;
    readonly AuthService _auth;
    bool _horsLigneAccepte;

    public AccueilViewModel(DonneesService donnees, AuthService auth)
    {
        _donnees = donnees;
        _auth = auth;
    }

    public ObservableCollection<UniteItem> Unites { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfficherContenu), nameof(AfficherErreur))]
    bool enChargement = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfficherContenu), nameof(AfficherErreur))]
    bool enErreur;

    [ObservableProperty]
    bool horsConnexion;

    [ObservableProperty]
    string semestre = string.Empty;

    [ObservableProperty]
    string initiales = string.Empty;

    [ObservableProperty]
    string infoStockage = string.Empty;

    [ObservableProperty]
    string derniereSynchro = string.Empty;

    [ObservableProperty]
    string codeErreur = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfficherSaisie))]
    SaisieEnCours? saisie;

    [ObservableProperty]
    bool aucuneUnite;

    public bool AfficherContenu => !EnChargement && !EnErreur;
    public bool AfficherErreur => !EnChargement && EnErreur;
    public bool AfficherSaisie => Saisie is not null;

    public async Task ChargerAsync()
    {
        EnChargement = Unites.Count == 0;
        EnErreur = false;
        try
        {
            if (_auth.UtilisateurCourant is null && !await _auth.RestaurerSessionAsync())
            {
                await AllerA("//connexion");
                return;
            }

            await _donnees.ChargerAsync();
            var utilisateur = _auth.UtilisateurCourant!;
            Semestre = _donnees.Base.Semestre;
            Initiales = utilisateur.Initiales;

            HorsConnexion = Connectivity.Current.NetworkAccess != NetworkAccess.Internet;
            var synchro = Preferences.Default.Get("derniere_synchro", DateTime.MinValue);
            if (!HorsConnexion)
            {
                synchro = DateTime.Now;
                Preferences.Default.Set("derniere_synchro", synchro);
            }
            DerniereSynchro = synchro == DateTime.MinValue
                ? "Aucune synchronisation"
                : $"Dernière synchronisation {Quand(synchro)}.";

            if (HorsConnexion && !_horsLigneAccepte)
            {
                CodeErreur = "Code : RESEAU_INDISPONIBLE";
                EnErreur = true;
                return;
            }

            InfoStockage = HorsConnexion
                ? "Données enregistrées sur l'appareil · hors connexion"
                : $"Données enregistrées sur l'appareil · synchronisé à {synchro:HH\\:mm}";

            Remplir(utilisateur.Role == Role.Enseignant);
        }
        catch (Exception)
        {
            CodeErreur = "Code : LECTURE_IMPOSSIBLE";
            EnErreur = true;
        }
        finally
        {
            EnChargement = false;
        }
    }

    void Remplir(bool enseignant)
    {
        Unites.Clear();
        SaisieEnCours? saisie = null;

        foreach (var ue in _donnees.Base.Unites)
        {
            if (!enseignant && !ue.Publiee)
                continue;

            var etudiants = _donnees.EtudiantsDe(ue.Promotion);
            var statut = Calculs.Statut(ue, etudiants);
            Unites.Add(new UniteItem
            {
                Code = ue.Code,
                Intitule = ue.Intitule,
                Details = $"{ue.Promotion} · {Calculs.Pluriel(etudiants.Count, "étudiant")} · {Calculs.Pluriel(ue.Evaluations.Count, "évaluation")}",
                Statut = statut switch
                {
                    StatutUe.Publie => "Publié",
                    StatutUe.EnCours => "En cours",
                    _ => "Brouillon"
                },
                VarianteStatut = statut switch
                {
                    StatutUe.Publie => VarianteEtiquette.Succes,
                    StatutUe.EnCours => VarianteEtiquette.Attention,
                    _ => VarianteEtiquette.Neutre
                },
                Moyenne = statut == StatutUe.EnCours ? "—" : Calculs.Format(Calculs.MoyenneUe(ue, etudiants))
            });

            if (enseignant && saisie is null && !ue.Publiee)
            {
                foreach (var evaluation in ue.Evaluations)
                {
                    var saisies = Calculs.NombreSaisies(evaluation, etudiants);
                    if (saisies > 0 && saisies < etudiants.Count)
                    {
                        saisie = new SaisieEnCours
                        {
                            Code = ue.Code,
                            EvaluationId = evaluation.Id,
                            Titre = $"{evaluation.Intitule} · {ue.Code}",
                            Progression = $"{saisies} notes sur {etudiants.Count} saisies",
                            Ratio = (double)saisies / etudiants.Count
                        };
                        break;
                    }
                }
            }
        }

        Saisie = saisie;
        AucuneUnite = Unites.Count == 0;
    }

    static string Quand(DateTime date) =>
        date.Date == DateTime.Today ? $"aujourd'hui à {date:HH\\:mm}"
        : date.Date == DateTime.Today.AddDays(-1) ? $"hier à {date:HH\\:mm}"
        : $"le {date.ToString("d MMMM", Calculs.Culture)} à {date:HH\\:mm}";

    [RelayCommand]
    async Task Reessayer()
    {
        EnChargement = true;
        await ChargerAsync();
    }

    [RelayCommand]
    async Task VoirBrouillons()
    {
        _horsLigneAccepte = true;
        await ChargerAsync();
    }

    [RelayCommand]
    Task Ouvrir(UniteItem item) =>
        _auth.EstEnseignant
            ? AllerA("ue", new Dictionary<string, object> { ["code"] = item.Code })
            : AllerA("moyennes", new Dictionary<string, object> { ["code"] = item.Code });

    [RelayCommand]
    Task Reprendre() =>
        Saisie is null
            ? Task.CompletedTask
            : AllerA("saisie", new Dictionary<string, object> { ["code"] = Saisie.Code, ["evaluation"] = Saisie.EvaluationId });

    [RelayCommand]
    Task Profil() => AllerA("//principal/profil");
}
