using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class EtudiantsPage : ContentPage
{
    readonly EtudiantsViewModel _vueModele;

    public EtudiantsPage(EtudiantsViewModel vueModele)
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
