using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class ConnexionPage : ContentPage
{
    public ConnexionPage(ConnexionViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = vueModele;
        ChampEmail.Valide += (_, _) => ChampMotDePasse.Focus();
        ChampMotDePasse.Valide += (_, _) =>
        {
            if (vueModele.ConnecterCommand.CanExecute(null))
                vueModele.ConnecterCommand.Execute(null);
        };
    }
}
