using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class UePage : ContentPage
{
    readonly UeViewModel _vueModele;

    public UePage(UeViewModel vueModele)
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
