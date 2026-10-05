namespace GestionNotesEtudiant.Models;

public enum StatutUe
{
    Brouillon,
    EnCours,
    Publie
}

public class UniteEnseignement
{
    public string Code { get; set; } = string.Empty;
    public string Intitule { get; set; } = string.Empty;
    public string Promotion { get; set; } = string.Empty;
    public int Credits { get; set; }
    public List<Evaluation> Evaluations { get; set; } = new();

    public bool Publiee { get; set; }
    public DateTime? DatePublication { get; set; }
    public bool NotifierEtudiants { get; set; } = true;
    public bool AfficherRang { get; set; }

    public int SommeCoefficients => Evaluations.Sum(e => e.Coefficient);
}
