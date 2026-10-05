using System.Security.Cryptography;
using System.Text.RegularExpressions;
using GestionNotesEtudiant.Models;

namespace GestionNotesEtudiant.Services;

public partial class AuthService
{
    const string CleSession = "session_email";

    readonly DonneesService _donnees;

    public AuthService(DonneesService donnees) => _donnees = donnees;

    public Utilisateur? UtilisateurCourant { get; private set; }

    public bool EstEnseignant => UtilisateurCourant?.Role == Role.Enseignant;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$")]
    private static partial Regex RegexEmail();

    public static bool EmailValide(string? email) => !string.IsNullOrWhiteSpace(email) && RegexEmail().IsMatch(email.Trim());

    public async Task<bool> RestaurerSessionAsync()
    {
        var email = Preferences.Default.Get(CleSession, string.Empty);
        if (string.IsNullOrEmpty(email))
            return false;

        await _donnees.ChargerAsync();
        UtilisateurCourant = Trouver(email);
        return UtilisateurCourant is not null;
    }

    public async Task<bool> ConnecterAsync(string email, string motDePasse)
    {
        await _donnees.ChargerAsync();
        var utilisateur = Trouver(email);
        if (utilisateur is null || !Verifier(motDePasse, utilisateur))
            return false;

        Ouvrir(utilisateur);
        return true;
    }

    public async Task<bool> InscrireAsync(string nomComplet, string email, string motDePasse, Role role)
    {
        await _donnees.ChargerAsync();
        if (Trouver(email) is not null)
            return false;

        var (sel, hache) = HacherMotDePasse(motDePasse);
        var utilisateur = new Utilisateur
        {
            NomComplet = nomComplet.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Sel = sel,
            MotDePasseHache = hache
        };
        _donnees.Base.Utilisateurs.Add(utilisateur);
        await _donnees.EnregistrerAsync();
        Ouvrir(utilisateur);
        return true;
    }

    public bool EmailExiste(string email) => _donnees.EstCharge && Trouver(email) is not null;

    public void Deconnecter()
    {
        UtilisateurCourant = null;
        Preferences.Default.Remove(CleSession);
    }

    void Ouvrir(Utilisateur utilisateur)
    {
        UtilisateurCourant = utilisateur;
        Preferences.Default.Set(CleSession, utilisateur.Email);
    }

    Utilisateur? Trouver(string email) =>
        _donnees.Base.Utilisateurs.FirstOrDefault(u =>
            string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

    public static (string Sel, string Hache) HacherMotDePasse(string motDePasse)
    {
        var sel = RandomNumberGenerator.GetBytes(16);
        var hache = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, 100_000, HashAlgorithmName.SHA256, 32);
        return (Convert.ToBase64String(sel), Convert.ToBase64String(hache));
    }

    static bool Verifier(string motDePasse, Utilisateur utilisateur)
    {
        var sel = Convert.FromBase64String(utilisateur.Sel);
        var attendu = Convert.FromBase64String(utilisateur.MotDePasseHache);
        var calcule = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(attendu, calcule);
    }
}
