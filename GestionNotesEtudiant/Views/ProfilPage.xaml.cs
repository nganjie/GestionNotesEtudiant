using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class ProfilPage : ContentPage
{
    readonly ProfilViewModel _vueModele;

    public ProfilPage(ProfilViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = _vueModele = vueModele;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vueModele.Charger();
    }
}
