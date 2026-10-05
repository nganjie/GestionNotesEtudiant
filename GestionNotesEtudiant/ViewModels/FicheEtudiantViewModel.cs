using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionNotesEtudiant.Controls;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public class ResultatUe
{
    public string Code { get; init; } = string.Empty;
    public string Intitule { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public string Moyenne { get; init; } = "—";
    public string Badge { get; init; } = string.Empty;
    public VarianteEtiquette VarianteBadge { get; init; }
}

public partial class FicheEtudiantViewModel : VueModeleBase, IQueryAttributable
{
    readonly DonneesService _donnees;

    public FicheEtudiantViewModel(DonneesService donnees) => _donnees = donnees;

    public ObservableCollection<ResultatUe> Resultats { get; } = new();

    [ObservableProperty] string nom = string.Empty;
    [ObservableProperty] string details = string.Empty;
    [ObservableProperty] bool aucunResultat;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("matricule", out var m) || m is not string matricule)
            return;
        var etudiant = _donnees.Etudiant(matricule);
        if (etudiant is null)
            return;

        Nom = etudiant.NomComplet;
        Details = $"Matricule {etudiant.Matricule} · {etudiant.Promotion}";

        Resultats.Clear();
        foreach (var ue in _donnees.Base.Unites.Where(u => u.Promotion == etudiant.Promotion && u.Evaluations.Count > 0))
        {
            var moyenne = Calculs.MoyenneEtudiant(ue, matricule);
            var absent = Calculs.EstAbsent(ue, matricule);
            Resultats.Add(new ResultatUe
            {
                Code = ue.Code,
                Intitule = ue.Intitule,
                Detail = ue.Publiee ? "Publié" : "Provisoire",
                Moyenne = Calculs.Format(moyenne),
                Badge = absent ? "ABS" : moyenne is null ? string.Empty : moyenne >= Calculs.SeuilValidation ? "Validé" : "NV",
                VarianteBadge = absent ? VarianteEtiquette.Neutre : moyenne >= Calculs.SeuilValidation ? VarianteEtiquette.Succes : VarianteEtiquette.Attention
            });
        }
        AucunResultat = Resultats.Count == 0;
    }
}
