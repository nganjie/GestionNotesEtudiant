using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class ConnexionViewModel : VueModeleBase
{
    readonly AuthService _auth;

    public ConnexionViewModel(AuthService auth) => _auth = auth;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnecterCommand))]
    string email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnecterCommand))]
    string motDePasse = string.Empty;

    [ObservableProperty]
    string? erreurEmail;

    [ObservableProperty]
    string? erreurConnexion;

    partial void OnEmailChanged(string value)
    {
        ErreurEmail = null;
        ErreurConnexion = null;
    }

    partial void OnMotDePasseChanged(string value) => ErreurConnexion = null;

    bool PeutConnecter() => !EstOccupe && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(MotDePasse);

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(EstOccupe))
            ConnecterCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(PeutConnecter))]
    async Task Connecter()
    {
        if (!AuthService.EmailValide(Email))
        {
            ErreurEmail = "Adresse incomplète. Exemple : nom@etablissement.cm";
            return;
        }

        EstOccupe = true;
        try
        {
            if (await _auth.ConnecterAsync(Email, MotDePasse))
            {
                MotDePasse = string.Empty;
                await AllerA("//principal/cours");
            }
            else
            {
                ErreurConnexion = "E-mail ou mot de passe incorrect.";
            }
        }
        finally
        {
            EstOccupe = false;
        }
    }

    [RelayCommand]
    Task CreerCompte() => AllerA("inscription");

    [RelayCommand]
    Task MotDePasseOublie() => AllerA("motdepasse", new Dictionary<string, object> { ["email"] = Email });
}
