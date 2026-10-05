using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class FicheEtudiantPage : ContentPage
{
    public FicheEtudiantPage(FicheEtudiantViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = vueModele;
    }
}
