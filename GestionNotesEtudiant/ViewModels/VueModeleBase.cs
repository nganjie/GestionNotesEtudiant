using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionNotesEtudiant.ViewModels;

public abstract partial class VueModeleBase : ObservableObject
{
    [ObservableProperty]
    bool estOccupe;

    protected static Task AllerA(string route, IDictionary<string, object>? parametres = null) =>
        parametres is null ? Shell.Current.GoToAsync(route) : Shell.Current.GoToAsync(route, parametres);

    protected static Task Retour() => Shell.Current.GoToAsync("..");

    protected static Task Alerte(string titre, string message) =>
        Shell.Current.DisplayAlertAsync(titre, message, "OK");
}
