using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Models;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class InscriptionViewModel : VueModeleBase
{
    readonly AuthService _auth;

    public InscriptionViewModel(AuthService auth) => _auth = auth;

    public IList<string> Roles { get; } = new[] { "Enseignant", "Étudiant" };

    [ObservableProperty]
    int indexRole;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurNom))]
    string nomComplet = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurEmail))]
    string email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HuitCaracteres), nameof(UneLettre), nameof(UnChiffre), nameof(MotDePasseValide), nameof(ErreurConfirmation))]
    string motDePasse = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurConfirmation))]
    string confirmation = string.Empty;

    // Un champ n'est signalé en erreur qu'après avoir été quitté une première fois.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurNom))]
    bool nomVisite;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurEmail))]
    bool emailVisite;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErreurConfirmation))]
    bool confirmationVisitee;

    [ObservableProperty]
    string? erreurServeur;

    public string? ErreurNom =>
        NomVisite && NomComplet.Trim().Length < 3 ? "Indiquez votre nom et votre prénom." : null;

    public string? ErreurEmail
    {
        get
        {
            if (!EmailVisite || string.IsNullOrWhiteSpace(Email))
                return null;
            if (!AuthService.EmailValide(Email))
                return "Adresse incomplète. Exemple : nom@etablissement.cm";
            if (_auth.EmailExiste(Email))
                return "Un compte existe déjà avec cette adresse.";
            return null;
        }
    }

    public bool HuitCaracteres => MotDePasse.Length >= 8;
    public bool UneLettre => MotDePasse.Any(char.IsLetter);
    public bool UnChiffre => MotDePasse.Any(char.IsDigit);
    public bool MotDePasseValide => HuitCaracteres && UneLettre && UnChiffre;

    public string? ErreurConfirmation =>
        ConfirmationVisitee && Confirmation.Length > 0 && Confirmation != MotDePasse
            ? "Les deux mots de passe ne correspondent pas."
            : null;

    public bool FormulaireValide =>
        NomComplet.Trim().Length >= 3
        && AuthService.EmailValide(Email) && !_auth.EmailExiste(Email)
        && MotDePasseValide
        && Confirmation == MotDePasse;

    public bool AfficherAide => !FormulaireValide && (NomVisite || EmailVisite || MotDePasse.Length > 0);

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(NomComplet) or nameof(Email) or nameof(MotDePasse) or nameof(Confirmation)
            or nameof(NomVisite) or nameof(EmailVisite) or nameof(EstOccupe))
        {
            OnPropertyChanged(nameof(FormulaireValide));
            OnPropertyChanged(nameof(AfficherAide));
            CreerCommand.NotifyCanExecuteChanged();
        }
        if (e.PropertyName is nameof(Email))
            ErreurServeur = null;
    }

    bool PeutCreer() => FormulaireValide && !EstOccupe;

    [RelayCommand(CanExecute = nameof(PeutCreer))]
    async Task Creer()
    {
        EstOccupe = true;
        try
        {
            var role = IndexRole == 0 ? Role.Enseignant : Role.Etudiant;
            if (await _auth.InscrireAsync(NomComplet, Email, MotDePasse, role))
                await AllerA("//principal/cours");
            else
                ErreurServeur = "Un compte existe déjà avec cette adresse.";
        }
        finally
        {
            EstOccupe = false;
        }
    }

    [RelayCommand]
    Task SeConnecter() => Retour();
}
