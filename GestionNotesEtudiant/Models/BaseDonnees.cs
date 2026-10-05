namespace GestionNotesEtudiant.Models;

/// <summary>Contenu du fichier local de l'application.</summary>
public class BaseDonnees
{
    public int Version { get; set; } = 1;
    public string Semestre { get; set; } = string.Empty;
    public List<Utilisateur> Utilisateurs { get; set; } = new();
    public List<Etudiant> Etudiants { get; set; } = new();
    public List<UniteEnseignement> Unites { get; set; } = new();
    public DateTime? DerniereSynchronisation { get; set; }
}
