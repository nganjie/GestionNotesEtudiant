using GestionNotesEtudiant.Views;

namespace GestionNotesEtudiant;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("inscription", typeof(InscriptionPage));
        Routing.RegisterRoute("motdepasse", typeof(MotDePasseOubliePage));
        Routing.RegisterRoute("ue", typeof(UePage));
        Routing.RegisterRoute("nouvelleevaluation", typeof(NouvelleEvaluationPage));
        Routing.RegisterRoute("saisie", typeof(SaisiePage));
        Routing.RegisterRoute("moyennes", typeof(MoyennesPage));
        Routing.RegisterRoute("publication", typeof(PublicationPage));
        Routing.RegisterRoute("export", typeof(ExportPage));
        Routing.RegisterRoute("etudiant", typeof(FicheEtudiantPage));

        // Session mémorisée : on ouvre directement les cours.
        if (Preferences.Default.ContainsKey("session_email"))
            CurrentItem = Principal;
    }
}
