using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class MoyennesPage : ContentPage
{
    readonly MoyennesViewModel _vueModele;

    public MoyennesPage(MoyennesViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = _vueModele = vueModele;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vueModele.ChargerAsync();
    }
}
