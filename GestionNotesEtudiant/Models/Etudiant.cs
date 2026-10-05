namespace GestionNotesEtudiant.Models;

public class Etudiant
{
    public string Matricule { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Promotion { get; set; } = string.Empty;

    public string NomComplet => $"{Nom} {Prenom}";
}
