using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Models;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class FiltrePromotion : ObservableObject
{
    public string Promotion { get; init; } = string.Empty;
    public string Libelle { get; init; } = string.Empty;

    [ObservableProperty] bool selectionne;
}

public partial class EtudiantsViewModel : VueModeleBase
{
    const int TaillePage = 20;

    readonly DonneesService _donnees;
    List<Etudiant> _filtres = new();

    public EtudiantsViewModel(DonneesService donnees) => _donnees = donnees;

    public ObservableCollection<FiltrePromotion> Promotions { get; } = new();
    public ObservableCollection<Etudiant> Etudiants { get; } = new();

    [ObservableProperty] string recherche = string.Empty;
    [ObservableProperty] bool afficherPlus;
    [ObservableProperty] string textePlus = string.Empty;
    [ObservableProperty] bool aucunResultat;

    public async Task ChargerAsync()
    {
        await _donnees.ChargerAsync();
        if (Promotions.Count == 0)
        {
            foreach (var promotion in _donnees.Promotions)
            {
                Promotions.Add(new FiltrePromotion
                {
                    Promotion = promotion,
                    Libelle = $"{promotion} · {_donnees.EtudiantsDe(promotion).Count}",
                    Selectionne = Promotions.Count == 0
                });
            }
        }
        Filtrer();
    }

    partial void OnRechercheChanged(string value) => Filtrer();

    [RelayCommand]
    void ChoisirPromotion(FiltrePromotion filtre)
    {
        foreach (var p in Promotions)
            p.Selectionne = p == filtre;
        Filtrer();
    }

    void Filtrer()
    {
        if (!_donnees.EstCharge)
            return;

        var promotion = Promotions.FirstOrDefault(p => p.Selectionne)?.Promotion;
        var terme = SansAccents(Recherche.Trim());
        _filtres = _donnees.Base.Etudiants
            .Where(e => promotion is null || e.Promotion == promotion)
            .Where(e => terme.Length == 0
                        || SansAccents(e.NomComplet).Contains(terme, StringComparison.OrdinalIgnoreCase)
                        || e.Matricule.Contains(terme, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
            .ToList();

        Etudiants.Clear();
        Page();
        AucunResultat = _filtres.Count == 0;
    }

    void Page()
    {
        foreach (var etudiant in _filtres.Skip(Etudiants.Count).Take(TaillePage))
            Etudiants.Add(etudiant);
        var restants = _filtres.Count - Etudiants.Count;
        AfficherPlus = restants > 0;
        TextePlus = $"Afficher {Math.Min(restants, TaillePage)} de plus";
    }

    [RelayCommand]
    void Plus() => Page();

    [RelayCommand]
    Task Ouvrir(Etudiant etudiant) =>
        AllerA("etudiant", new Dictionary<string, object> { ["matricule"] = etudiant.Matricule });

    static string SansAccents(string texte)
    {
        var decompose = texte.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decompose.Length);
        foreach (var c in decompose)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString();
    }
}
