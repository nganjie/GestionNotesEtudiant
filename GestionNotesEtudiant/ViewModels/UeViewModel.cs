using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Controls;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public class EvaluationItem
{
    public string Id { get; init; } = string.Empty;
    public int Coefficient { get; init; }
    public string Intitule { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public string Moyenne { get; init; } = "—";
    public string Statut { get; init; } = string.Empty;
    public VarianteEtiquette VarianteStatut { get; init; }
}

public partial class UeViewModel : VueModeleBase, IQueryAttributable
{
    readonly DonneesService _donnees;
    string _code = string.Empty;

    public UeViewModel(DonneesService donnees) => _donnees = donnees;

    public ObservableCollection<EvaluationItem> Evaluations { get; } = new();

    [ObservableProperty] string code = string.Empty;
    [ObservableProperty] string intitule = string.Empty;
    [ObservableProperty] string details = string.Empty;
    [ObservableProperty] string moyenneProvisoire = "—";
    [ObservableProperty] string sommeCoefficients = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AfficherEvaluations))]
    bool estVide;

    public bool AfficherEvaluations => !EstVide;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("code", out var valeur) && valeur is string c)
            _code = c;
    }

    public async Task ChargerAsync()
    {
        await _donnees.ChargerAsync();
        var ue = _donnees.Unite(_code);
        if (ue is null)
            return;

        var etudiants = _donnees.EtudiantsDe(ue.Promotion);
        Code = ue.Code;
        Intitule = ue.Intitule;
        Details = $"{ue.Promotion} · {Calculs.Pluriel(etudiants.Count, "étudiant")} · {Calculs.Pluriel(ue.Credits, "crédit")}";
        MoyenneProvisoire = Calculs.Format(Calculs.MoyenneUe(ue, etudiants));
        SommeCoefficients = ue.SommeCoefficients.ToString();

        Evaluations.Clear();
        foreach (var evaluation in ue.Evaluations.OrderBy(e => e.Date))
        {
            var saisies = Calculs.NombreSaisies(evaluation, etudiants);
            var complete = saisies == etudiants.Count && etudiants.Count > 0;
            Evaluations.Add(new EvaluationItem
            {
                Id = evaluation.Id,
                Coefficient = evaluation.Coefficient,
                Intitule = evaluation.Intitule,
                Details = $"{Calculs.DateCourte(evaluation.Date)} · {saisies} / {etudiants.Count} saisies",
                // La moyenne d'une évaluation n'est affichée qu'une fois toutes les notes saisies.
                Moyenne = complete ? Calculs.Format(Calculs.MoyenneEvaluation(evaluation, etudiants)) : "—",
                Statut = complete ? "Complet" : saisies == 0 ? "À saisir" : "En cours",
                VarianteStatut = complete ? VarianteEtiquette.Succes : saisies == 0 ? VarianteEtiquette.Neutre : VarianteEtiquette.Attention
            });
        }
        EstVide = Evaluations.Count == 0;
    }

    Dictionary<string, object> Parametres() => new() { ["code"] = _code };

    [RelayCommand]
    Task Ajouter() => AllerA("nouvelleevaluation", Parametres());

    [RelayCommand]
    Task OuvrirEvaluation(EvaluationItem item) =>
        AllerA("saisie", new Dictionary<string, object> { ["code"] = _code, ["evaluation"] = item.Id });

    [RelayCommand]
    Task VoirMoyennes() => AllerA("moyennes", Parametres());

    [RelayCommand]
    Task Exporter() => AllerA("export", Parametres());
}
