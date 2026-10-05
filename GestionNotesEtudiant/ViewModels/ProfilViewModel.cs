using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Models;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class ProfilViewModel : VueModeleBase
{
    readonly AuthService _auth;
    readonly DonneesService _donnees;

    public ProfilViewModel(AuthService auth, DonneesService donnees)
    {
        _auth = auth;
        _donnees = donnees;
    }

    [ObservableProperty] string initiales = string.Empty;
    [ObservableProperty] string nom = string.Empty;
    [ObservableProperty] string email = string.Empty;
    [ObservableProperty] string role = string.Empty;
    [ObservableProperty] string semestre = string.Empty;
    [ObservableProperty] string resume = string.Empty;

    public void Charger()
    {
        var utilisateur = _auth.UtilisateurCourant;
        if (utilisateur is null || !_donnees.EstCharge)
            return;

        Initiales = utilisateur.Initiales;
        Nom = utilisateur.NomComplet;
        Email = utilisateur.Email;
        Role = utilisateur.Role == Models.Role.Enseignant ? "Enseignant" : "Étudiant";
        Semestre = _donnees.Base.Semestre;

        var unites = _donnees.Base.Unites;
        Resume = $"{Calculs.Pluriel(unites.Count, "unité")} d'enseignement · {Calculs.Pluriel(unites.Count(u => u.Publiee), "publiée")}";
    }

    [RelayCommand]
    async Task Deconnecter()
    {
        if (!await Shell.Current.DisplayAlertAsync("Se déconnecter", "Vos notes restent enregistrées sur cet appareil.", "Se déconnecter", "Annuler"))
            return;
        _auth.Deconnecter();
        await AllerA("//connexion");
    }
}
