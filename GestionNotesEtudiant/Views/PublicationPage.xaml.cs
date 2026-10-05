using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class PublicationPage : ContentPage
{
    public PublicationPage(PublicationViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = vueModele;
    }
}
