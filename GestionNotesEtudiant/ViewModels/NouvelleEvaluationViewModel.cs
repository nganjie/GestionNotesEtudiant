using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Models;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class NouvelleEvaluationViewModel : VueModeleBase, IQueryAttributable
{
    const int CoefficientMax = 10;

    readonly DonneesService _donnees;
    string _code = string.Empty;
    int _sommeActuelle;

    public NouvelleEvaluationViewModel(DonneesService donnees) => _donnees = donnees;

    public IList<string> Types { get; } = new[] { "Contrôle", "TP", "Examen" };

    [ObservableProperty] string sousTitre = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurIntitule))]
    string intitule = string.Empty;

    [ObservableProperty] bool intituleVisite;

    [ObservableProperty] int indexType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InfoCoefficients), nameof(PeutDiminuer), nameof(PeutAugmenter))]
    int coefficient = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurNoteSur))]
    string noteSur = "20";

    [ObservableProperty] DateTime date = DateTime.Today;

    public string? ErreurIntitule =>
        IntituleVisite && string.IsNullOrWhiteSpace(Intitule) ? "Donnez un intitulé à l'évaluation." : null;

    public string? ErreurNoteSur =>
        Calculs.TryLireNote(NoteSur, out var v) && v > 0 && v <= 100 ? null : "Entre 1 et 100";

    public bool PeutDiminuer => Coefficient > 1;
    public bool PeutAugmenter => Coefficient < CoefficientMax;

    public string InfoCoefficients =>
        $"La somme des coefficients de l'UE passera de {_sommeActuelle} à {_sommeActuelle + Coefficient}. " +
        "Les moyennes seront recalculées automatiquement.";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("code", out var valeur) && valeur is string c)
            _code = c;

        var ue = _donnees.Unite(_code);
        if (ue is null)
            return;
        SousTitre = $"{ue.Code} · {ue.Intitule}";
        _sommeActuelle = ue.SommeCoefficients;
        OnPropertyChanged(nameof(InfoCoefficients));
    }

    partial void OnIntituleChanged(string value) => CreerCommand.NotifyCanExecuteChanged();
    partial void OnNoteSurChanged(string value) => CreerCommand.NotifyCanExecuteChanged();
    partial void OnIntituleVisiteChanged(bool value) => OnPropertyChanged(nameof(ErreurIntitule));

    [RelayCommand]
    void Diminuer()
    {
        if (PeutDiminuer)
            Coefficient--;
    }

    [RelayCommand]
    void Augmenter()
    {
        if (PeutAugmenter)
            Coefficient++;
    }

    bool PeutCreer() => !string.IsNullOrWhiteSpace(Intitule) && ErreurNoteSur is null && !EstOccupe;

    [RelayCommand(CanExecute = nameof(PeutCreer))]
    async Task Creer()
    {
        var ue = _donnees.Unite(_code);
        if (ue is null || !Calculs.TryLireNote(NoteSur, out var bareme))
            return;

        EstOccupe = true;
        try
        {
            var evaluation = new Evaluation
            {
                Intitule = Intitule.Trim(),
                Type = (TypeEvaluation)IndexType,
                Coefficient = Coefficient,
                NoteSur = bareme,
                Date = Date
            };
            ue.Evaluations.Add(evaluation);
            await _donnees.EnregistrerAsync();
            await Retour();
        }
        finally
        {
            EstOccupe = false;
        }
    }

    [RelayCommand]
    Task Annuler() => Retour();
}
