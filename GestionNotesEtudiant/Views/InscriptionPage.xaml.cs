using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class InscriptionPage : ContentPage
{
    public InscriptionPage(InscriptionViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = vueModele;

        ChampNom.FocusPerdu += (_, _) => vueModele.NomVisite = true;
        ChampEmail.FocusPerdu += (_, _) => vueModele.EmailVisite = true;
        ChampConfirmation.FocusPerdu += (_, _) => vueModele.ConfirmationVisitee = true;

        ChampNom.Valide += (_, _) => ChampEmail.Focus();
        ChampEmail.Valide += (_, _) => ChampMotDePasse.Focus();
        ChampMotDePasse.Valide += (_, _) =>
        {
            if (vueModele.MotDePasseValide)
                ChampConfirmation.Focus();
        };
    }
}
