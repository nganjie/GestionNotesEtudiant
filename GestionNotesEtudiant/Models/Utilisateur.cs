namespace GestionNotesEtudiant.Models;

public enum Role
{
    Enseignant,
    Etudiant
}

public class Utilisateur
{
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasseHache { get; set; } = string.Empty;
    public string Sel { get; set; } = string.Empty;
    public Role Role { get; set; }

    public string Initiales
    {
        get
        {
            var mots = NomComplet.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return mots.Length switch
            {
                0 => "?",
                1 => mots[0][..1].ToUpperInvariant(),
                _ => $"{mots[0][0]}{mots[^1][0]}".ToUpperInvariant()
            };
        }
    }
}
