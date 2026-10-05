using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class NouvelleEvaluationPage : ContentPage
{
    public NouvelleEvaluationPage(NouvelleEvaluationViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = vueModele;
        ChampIntitule.FocusPerdu += (_, _) => vueModele.IntituleVisite = true;
    }
}
