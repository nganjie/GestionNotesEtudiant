using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class MotDePasseOubliePage : ContentPage
{
    public MotDePasseOubliePage(MotDePasseOublieViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = vueModele;
    }
}
