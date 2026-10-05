namespace GestionNotesEtudiant.Models;

public enum TypeEvaluation
{
    Controle,
    TP,
    Examen
}

public class NoteEtudiant
{
    public double? Valeur { get; set; }
    public bool Absent { get; set; }

    public bool EstSaisie => Absent || Valeur.HasValue;
}

public class Evaluation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Intitule { get; set; } = string.Empty;
    public TypeEvaluation Type { get; set; }
    public int Coefficient { get; set; } = 1;
    public double NoteSur { get; set; } = 20;
    public DateTime Date { get; set; } = DateTime.Today;
    public bool Terminee { get; set; }

    /// <summary>Notes indexées par matricule.</summary>
    public Dictionary<string, NoteEtudiant> Notes { get; set; } = new();

    public string Abreviation => Type switch
    {
        TypeEvaluation.Controle => "CC",
        TypeEvaluation.TP => "TP",
        _ => "Ex."
    };

    public string LibelleCourt => Type switch
    {
        TypeEvaluation.Controle => "CC",
        TypeEvaluation.TP => "TP",
        _ => "Examen"
    };

    public NoteEtudiant? NoteDe(string matricule) =>
        Notes.TryGetValue(matricule, out var note) ? note : null;

    /// <summary>Note ramenée sur 20, ou null si non saisie ou absent.</summary>
    public double? NoteSur20(string matricule)
    {
        var note = NoteDe(matricule);
        if (note?.Valeur is not double valeur || note.Absent || NoteSur <= 0)
            return null;
        return valeur * 20 / NoteSur;
    }
}
