using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Controls;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public class LigneMoyenne
{
    public string Matricule { get; init; } = string.Empty;
    public string Nom { get; init; } = string.Empty;
    public string Badge { get; init; } = string.Empty;
    public VarianteEtiquette VarianteBadge { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();
    public string Moyenne { get; init; } = "—";
    public Color CouleurMoyenne { get; init; } = Colors.Black;
}

public partial class MoyennesViewModel : VueModeleBase, IQueryAttributable
{
    const int TaillePage = 20;

    readonly DonneesService _donnees;
    readonly AuthService _auth;
    readonly List<LigneMoyenne> _toutes = new();
    string _code = string.Empty;

    public MoyennesViewModel(DonneesService donnees, AuthService auth)
    {
        _donnees = donnees;
        _auth = auth;
    }

    public ObservableCollection<string> Colonnes { get; } = new();
    public ObservableCollection<LigneMoyenne> Lignes { get; } = new();

    [ObservableProperty] string sousTitre = string.Empty;
    [ObservableProperty] string statut = string.Empty;
    [ObservableProperty] VarianteEtiquette varianteStatut;
    [ObservableProperty] string moyenneUe = "—";
    [ObservableProperty] string valides = "—";
    [ObservableProperty] string noteMax = "—";
    [ObservableProperty] string texteSuivants = string.Empty;
    [ObservableProperty] bool afficherSuivants;
    [ObservableProperty] bool estEnseignant;
    [ObservableProperty] string texteBoutonPublier = "Publier";
    [ObservableProperty] bool aucunEtudiant;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("code", out var c) && c is string code)
            _code = code;
    }

    public async Task ChargerAsync()
    {
        await _donnees.ChargerAsync();
        var ue = _donnees.Unite(_code);
        if (ue is null)
            return;

        EstEnseignant = _auth.EstEnseignant;
        var etudiants = _donnees.EtudiantsDe(ue.Promotion);
        SousTitre = $"{ue.Code} · {ue.Intitule}";
        Statut = ue.Publiee ? "Publié" : "Non publié";
        VarianteStatut = ue.Publiee ? VarianteEtiquette.Succes : VarianteEtiquette.Neutre;
        TexteBoutonPublier = ue.Publiee ? "Republier" : "Publier";

        Colonnes.Clear();
        foreach (var evaluation in ue.Evaluations)
            Colonnes.Add(evaluation.Abreviation);

        _toutes.Clear();
        var moyennes = new List<double>();
        foreach (var etudiant in etudiants)
        {
            var moyenne = Calculs.MoyenneEtudiant(ue, etudiant.Matricule);
            var absent = Calculs.EstAbsent(ue, etudiant.Matricule);
            if (moyenne is double m)
                moyennes.Add(m);

            var nonValide = moyenne < Calculs.SeuilValidation;
            _toutes.Add(new LigneMoyenne
            {
                Matricule = etudiant.Matricule,
                Nom = etudiant.NomComplet,
                Badge = absent ? "ABS" : nonValide ? "NV" : string.Empty,
                VarianteBadge = absent ? VarianteEtiquette.Neutre : VarianteEtiquette.Attention,
                Notes = ue.Evaluations.Select(e => e.NoteDe(etudiant.Matricule) switch
                {
                    { Absent: true } => "ABS",
                    { Valeur: double v } => Calculs.FormatNote(v * 20 / e.NoteSur),
                    _ => "–"
                }).ToList(),
                Moyenne = Calculs.Format(moyenne),
                CouleurMoyenne = nonValide ? Theme.Attention : Theme.Encre
            });
        }

        MoyenneUe = Calculs.Format(moyennes.Count > 0 ? moyennes.Average() : null);
        Valides = $"{moyennes.Count(m => m >= Calculs.SeuilValidation)} / {etudiants.Count}";
        NoteMax = Calculs.Format(moyennes.Count > 0 ? moyennes.Max() : null);
        AucunEtudiant = etudiants.Count == 0;

        Lignes.Clear();
        AfficherPage(8);
    }

    void AfficherPage(int nombre)
    {
        foreach (var ligne in _toutes.Skip(Lignes.Count).Take(nombre))
            Lignes.Add(ligne);
        var restants = _toutes.Count - Lignes.Count;
        AfficherSuivants = restants > 0;
        TexteSuivants = $"Afficher les {Math.Min(restants, TaillePage)} suivants";
    }

    [RelayCommand]
    void Suivants() => AfficherPage(TaillePage);

    Dictionary<string, object> Parametres() => new() { ["code"] = _code };

    [RelayCommand]
    Task Exporter() => AllerA("export", Parametres());

    [RelayCommand]
    Task Publier() => AllerA("publication", Parametres());
}
