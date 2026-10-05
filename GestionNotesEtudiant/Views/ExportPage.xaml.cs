using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class ExportPage : ContentPage
{
    readonly ExportViewModel _vueModele;

    public ExportPage(ExportViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = _vueModele = vueModele;
    }

    /// <summary>Toucher le libellé coche ou décoche la case correspondante.</summary>
    void OnLibelleTouche(object? sender, TappedEventArgs e)
    {
        switch (e.Parameter as string)
        {
            case nameof(ExportViewModel.NotesParEvaluation): _vueModele.NotesParEvaluation = !_vueModele.NotesParEvaluation; break;
            case nameof(ExportViewModel.Moyennes): _vueModele.Moyennes = !_vueModele.Moyennes; break;
            case nameof(ExportViewModel.Statistiques): _vueModele.Statistiques = !_vueModele.Statistiques; break;
            case nameof(ExportViewModel.Anonymiser): _vueModele.Anonymiser = !_vueModele.Anonymiser; break;
        }
    }
}
