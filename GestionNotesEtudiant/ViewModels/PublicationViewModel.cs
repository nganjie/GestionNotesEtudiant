using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public class Verification
{
    public string Icone { get; init; } = "ic_coche_succes.png";
    public string Titre { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public bool Separateur { get; init; } = true;
}

public partial class PublicationViewModel : VueModeleBase, IQueryAttributable
{
    readonly DonneesService _donnees;
    string _code = string.Empty;

    public PublicationViewModel(DonneesService donnees) => _donnees = donnees;

    public ObservableCollection<Verification> Verifications { get; } = new();

    [ObservableProperty] string sousTitre = string.Empty;
    [ObservableProperty] bool notifierEtudiants = true;
    [ObservableProperty] bool afficherRang;
    [ObservableProperty] string avertissement = string.Empty;
    [ObservableProperty] string texteBouton = "Publier";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PublierCommand))]
    bool peutPublier;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("code", out var c) && c is string code)
            _code = code;
        Charger();
    }

    void Charger()
    {
        var ue = _donnees.Unite(_code);
        if (ue is null)
            return;

        var etudiants = _donnees.EtudiantsDe(ue.Promotion);
        SousTitre = $"{ue.Code} · {ue.Intitule}";
        NotifierEtudiants = ue.NotifierEtudiants;
        AfficherRang = ue.AfficherRang;

        var incompletes = ue.Evaluations.Where(e => !Calculs.EstComplete(e, etudiants)).ToList();
        var absents = etudiants.Count(e => Calculs.EstAbsent(ue, e.Matricule));

        var liste = new List<Verification>();
        if (ue.Evaluations.Count == 0)
        {
            liste.Add(new Verification { Icone = "ic_alerte_attention.png", Titre = "Aucune évaluation", Detail = "Créez au moins une évaluation" });
        }
        else if (incompletes.Count == 0)
        {
            liste.Add(new Verification
            {
                Titre = "Toutes les évaluations sont saisies",
                Detail = $"{Calculs.Pluriel(ue.Evaluations.Count, "évaluation")} · {Calculs.Pluriel(etudiants.Count, "étudiant")}"
            });
        }
        else
        {
            liste.Add(new Verification
            {
                Icone = "ic_alerte_attention.png",
                Titre = incompletes.Count == 1 ? "1 évaluation incomplète" : $"{incompletes.Count} évaluations incomplètes",
                Detail = string.Join(" · ", incompletes.Select(e => $"{e.Intitule} : {Calculs.NombreSaisies(e, etudiants)} / {etudiants.Count}"))
            });
        }

        if (ue.Evaluations.Count > 0)
        {
            liste.Add(new Verification
            {
                Titre = "Coefficients vérifiés",
                Detail = string.Join(" · ", ue.Evaluations.Select(e => $"{e.LibelleCourt} {e.Coefficient}"))
            });
        }

        if (absents > 0)
        {
            liste.Add(new Verification
            {
                Icone = "ic_alerte_attention.png",
                Titre = absents == 1 ? "1 étudiant absent" : $"{absents} étudiants absents",
                Detail = absents == 1 ? "Sa moyenne ne sera pas calculée" : "Leur moyenne ne sera pas calculée"
            });
        }

        Verifications.Clear();
        for (int i = 0; i < liste.Count; i++)
        {
            var v = liste[i];
            Verifications.Add(new Verification { Icone = v.Icone, Titre = v.Titre, Detail = v.Detail, Separateur = i < liste.Count - 1 });
        }

        Avertissement = $"Une fois publiées, les notes sont visibles par les étudiants de {ue.Promotion}. " +
                        "Toute modification ultérieure leur sera signalée.";
        TexteBouton = $"Publier pour {Calculs.Pluriel(etudiants.Count, "étudiant")}";
        PeutPublier = ue.Evaluations.Count > 0 && incompletes.Count == 0;
    }

    [RelayCommand(CanExecute = nameof(PeutPublier))]
    async Task Publier()
    {
        var ue = _donnees.Unite(_code);
        if (ue is null)
            return;

        ue.Publiee = true;
        ue.DatePublication = DateTime.Now;
        ue.NotifierEtudiants = NotifierEtudiants;
        ue.AfficherRang = AfficherRang;
        await _donnees.EnregistrerAsync();
        await Alerte("Notes publiées", $"Les moyennes de {ue.Code} sont désormais visibles par les étudiants.");
        await AllerA("../..");
    }

    [RelayCommand]
    Task PlusTard() => Retour();
}
