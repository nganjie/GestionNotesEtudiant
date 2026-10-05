using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class AccueilPage : ContentPage
{
    readonly AccueilViewModel _vueModele;

    public AccueilPage(AccueilViewModel vueModele)
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
