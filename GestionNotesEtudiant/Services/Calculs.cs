using System.Globalization;
using GestionNotesEtudiant.Models;

namespace GestionNotesEtudiant.Services;

/// <summary>Calcul des moyennes pondérées par les coefficients.</summary>
public static class Calculs
{
    public const double SeuilValidation = 10;

    static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static bool EstAbsent(UniteEnseignement ue, string matricule) =>
        ue.Evaluations.Any(e => e.NoteDe(matricule)?.Absent == true);

    /// <summary>
    /// Moyenne d'un étudiant sur 20. Un étudiant absent à une évaluation n'a pas de moyenne.
    /// Tant que toutes les notes ne sont pas saisies, la moyenne est provisoire
    /// et ne porte que sur les évaluations notées.
    /// </summary>
    public static double? MoyenneEtudiant(UniteEnseignement ue, string matricule)
    {
        if (EstAbsent(ue, matricule))
            return null;

        double somme = 0;
        int coefficients = 0;
        foreach (var evaluation in ue.Evaluations)
        {
            if (evaluation.NoteSur20(matricule) is double note)
            {
                somme += note * evaluation.Coefficient;
                coefficients += evaluation.Coefficient;
            }
        }
        return coefficients == 0 ? null : somme / coefficients;
    }

    public static double? MoyenneUe(UniteEnseignement ue, IEnumerable<Etudiant> etudiants)
    {
        var moyennes = etudiants
            .Select(e => MoyenneEtudiant(ue, e.Matricule))
            .OfType<double>()
            .ToList();
        return moyennes.Count == 0 ? null : moyennes.Average();
    }

    public static double? MoyenneEvaluation(Evaluation evaluation, IEnumerable<Etudiant> etudiants)
    {
        var notes = etudiants
            .Select(e => evaluation.NoteSur20(e.Matricule))
            .OfType<double>()
            .ToList();
        return notes.Count == 0 ? null : notes.Average();
    }

    public static int NombreSaisies(Evaluation evaluation, IEnumerable<Etudiant> etudiants) =>
        etudiants.Count(e => evaluation.NoteDe(e.Matricule)?.EstSaisie == true);

    public static bool EstComplete(Evaluation evaluation, IReadOnlyCollection<Etudiant> etudiants) =>
        etudiants.Count > 0 && NombreSaisies(evaluation, etudiants) == etudiants.Count;

    public static StatutUe Statut(UniteEnseignement ue, IReadOnlyCollection<Etudiant> etudiants)
    {
        if (ue.Publiee)
            return StatutUe.Publie;
        if (ue.Evaluations.Count > 0 && ue.Evaluations.Any(e => !EstComplete(e, etudiants)))
            return StatutUe.EnCours;
        return StatutUe.Brouillon;
    }

    /// <summary>Format français, deux décimales : 12,78.</summary>
    public static string Format(double? valeur) =>
        valeur is double v ? Arrondir(v).ToString("0.00", Fr) : "—";

    /// <summary>Format d'une note saisie : 11,5 ou 14.</summary>
    public static string FormatNote(double valeur) => valeur.ToString("0.##", Fr);

    public static double Arrondir(double valeur) => Math.Round(valeur, 2, MidpointRounding.ToEven);

    /// <summary>Accepte « 11,5 » comme « 11.5 ».</summary>
    public static bool TryLireNote(string? texte, out double valeur)
    {
        valeur = 0;
        if (string.IsNullOrWhiteSpace(texte))
            return false;
        var normalise = texte.Trim().Replace(',', '.');
        return double.TryParse(normalise, NumberStyles.Number, CultureInfo.InvariantCulture, out valeur);
    }

    public static string Pluriel(int nombre, string singulier, string? pluriel = null) =>
        $"{nombre} {(nombre > 1 ? pluriel ?? singulier + "s" : singulier)}";

    /// <summary>« 28 sept. », « 19 oct. 2026 » avec l'année.</summary>
    public static string DateCourte(DateTime date, bool avecAnnee = false) =>
        date.ToString(avecAnnee ? "d MMM yyyy" : "d MMM", Fr);

    public static CultureInfo Culture => Fr;
}
