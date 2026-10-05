using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class MotDePasseOublieViewModel : VueModeleBase, IQueryAttributable
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnvoyerCommand))]
    string email = string.Empty;

    [ObservableProperty]
    string? erreurEmail;

    [ObservableProperty]
    bool envoye;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("email", out var valeur) && valeur is string e)
            Email = e;
    }

    partial void OnEmailChanged(string value) => ErreurEmail = null;

    bool PeutEnvoyer() => !string.IsNullOrWhiteSpace(Email);

    [RelayCommand(CanExecute = nameof(PeutEnvoyer))]
    void Envoyer()
    {
        if (!AuthService.EmailValide(Email))
        {
            ErreurEmail = "Adresse incomplète. Exemple : nom@etablissement.cm";
            return;
        }
        Envoye = true;
    }

    [RelayCommand]
    Task RetourConnexion() => Retour();
}
