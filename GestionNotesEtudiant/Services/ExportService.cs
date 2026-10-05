using System.Text;
using GestionNotesEtudiant.Models;

namespace GestionNotesEtudiant.Services;

public enum FormatExport
{
    Pdf,
    Csv,
    Excel
}

public record OptionsExport(bool NotesParEvaluation, bool Moyennes, bool Statistiques, bool Anonymiser);

/// <summary>Tableau indépendant du format de sortie.</summary>
public class TableauExport
{
    public string Titre { get; init; } = string.Empty;
    public string SousTitre { get; init; } = string.Empty;
    public List<string> Colonnes { get; } = new();
    public List<string[]> Lignes { get; } = new();
    public List<(string Libelle, string Valeur)> Statistiques { get; } = new();

    /// <summary>Pour Excel : coefficients des colonnes de notes et index de la colonne moyenne.</summary>
    public List<(int Colonne, int Coefficient)> ColonnesNotes { get; } = new();
    public int ColonneMoyenne { get; set; } = -1;
}

public class ExportService
{
    readonly DonneesService _donnees;

    public ExportService(DonneesService donnees) => _donnees = donnees;

    public TableauExport Construire(UniteEnseignement ue, OptionsExport options)
    {
        var etudiants = _donnees.EtudiantsDe(ue.Promotion);
        var tableau = new TableauExport
        {
            Titre = $"{ue.Code} · {ue.Intitule}",
            SousTitre = $"{ue.Promotion} · {_donnees.Base.Semestre} · édité le {DateTime.Now.ToString("d MMMM yyyy", Calculs.Culture)}"
        };

        tableau.Colonnes.Add("Matricule");
        if (!options.Anonymiser)
            tableau.Colonnes.Add("Nom et prénom");

        // Les formules Excel ne sont écrites que si toutes les notes sont sur 20.
        var formulesPossibles = ue.Evaluations.All(e => e.NoteSur == 20);
        if (options.NotesParEvaluation)
        {
            foreach (var evaluation in ue.Evaluations)
            {
                if (formulesPossibles)
                    tableau.ColonnesNotes.Add((tableau.Colonnes.Count, evaluation.Coefficient));
                tableau.Colonnes.Add($"{evaluation.Abreviation} ×{evaluation.Coefficient}");
            }
        }

        if (options.Moyennes)
        {
            tableau.ColonneMoyenne = tableau.Colonnes.Count;
            tableau.Colonnes.Add("Moy. /20");
            tableau.Colonnes.Add("Décision");
        }

        foreach (var etudiant in etudiants)
        {
            var ligne = new List<string> { etudiant.Matricule };
            if (!options.Anonymiser)
                ligne.Add(etudiant.NomComplet);

            if (options.NotesParEvaluation)
            {
                foreach (var evaluation in ue.Evaluations)
                {
                    var note = evaluation.NoteDe(etudiant.Matricule);
                    ligne.Add(note switch
                    {
                        { Absent: true } => "ABS",
                        { Valeur: double v } => Calculs.FormatNote(v),
                        _ => ""
                    });
                }
            }

            if (options.Moyennes)
            {
                var moyenne = Calculs.MoyenneEtudiant(ue, etudiant.Matricule);
                ligne.Add(moyenne is null ? "" : Calculs.Format(moyenne));
                ligne.Add(moyenne switch
                {
                    null when Calculs.EstAbsent(ue, etudiant.Matricule) => "ABS",
                    null => "",
                    >= Calculs.SeuilValidation => "Validé",
                    _ => "Non validé"
                });
            }

            tableau.Lignes.Add(ligne.ToArray());
        }

        if (options.Statistiques)
        {
            var moyennes = etudiants.Select(e => Calculs.MoyenneEtudiant(ue, e.Matricule)).OfType<double>().ToList();
            tableau.Statistiques.Add(("Effectif", etudiants.Count.ToString()));
            tableau.Statistiques.Add(("Moyenne de l'UE", Calculs.Format(moyennes.Count > 0 ? moyennes.Average() : null)));
            tableau.Statistiques.Add(("Note maximale", Calculs.Format(moyennes.Count > 0 ? moyennes.Max() : null)));
            tableau.Statistiques.Add(("Note minimale", Calculs.Format(moyennes.Count > 0 ? moyennes.Min() : null)));
            tableau.Statistiques.Add(("Validés (≥ 10)", $"{moyennes.Count(m => m >= Calculs.SeuilValidation)} / {etudiants.Count}"));
        }

        return tableau;
    }

    public byte[] Generer(TableauExport tableau, FormatExport format) => format switch
    {
        FormatExport.Pdf => PdfSimple.Generer(tableau),
        FormatExport.Excel => XlsxSimple.Generer(tableau),
        _ => GenererCsv(tableau)
    };

    public static string Extension(FormatExport format) => format switch
    {
        FormatExport.Pdf => "pdf",
        FormatExport.Excel => "xlsx",
        _ => "csv"
    };

    public static string TypeMime(FormatExport format) => format switch
    {
        FormatExport.Pdf => "application/pdf",
        FormatExport.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        _ => "text/csv"
    };

    public async Task<string> EcrireFichierAsync(UniteEnseignement ue, TableauExport tableau, FormatExport format)
    {
        var nom = $"notes-{ue.Code.Replace(" ", string.Empty).ToLowerInvariant()}-{DateTime.Now:yyyyMMdd-HHmm}.{Extension(format)}";
        var chemin = Path.Combine(FileSystem.CacheDirectory, nom);
        await File.WriteAllBytesAsync(chemin, Generer(tableau, format));
        return chemin;
    }

    static byte[] GenererCsv(TableauExport tableau)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(';', tableau.Colonnes.Select(Echapper)));
        foreach (var ligne in tableau.Lignes)
            sb.AppendLine(string.Join(';', ligne.Select(Echapper)));

        if (tableau.Statistiques.Count > 0)
        {
            sb.AppendLine();
            foreach (var (libelle, valeur) in tableau.Statistiques)
                sb.AppendLine($"{Echapper(libelle)};{Echapper(valeur)}");
        }

        // BOM pour une ouverture correcte des accents dans un tableur.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    static string Echapper(string valeur) =>
        valeur.IndexOfAny(new[] { ';', '"', '\n' }) >= 0 ? $"\"{valeur.Replace("\"", "\"\"")}\"" : valeur;
}
