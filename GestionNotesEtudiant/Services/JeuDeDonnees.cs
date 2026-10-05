using GestionNotesEtudiant.Models;

namespace GestionNotesEtudiant.Services;

/// <summary>Données de démonstration créées au premier lancement.</summary>
public static class JeuDeDonnees
{
    public const string EmailDemo = "m.etoundi@univ-exemple.cm";
    public const string MotDePasseDemo = "Bareme2026";

    static readonly string[] Noms =
    {
        "Abanda", "Akono", "Alima", "Amougou", "Atangana", "Ayissi", "Bella", "Bekono", "Bilong", "Daouda",
        "Ebode", "Edimo", "Ekane", "Ekotto", "Elong", "Embolo", "Eyenga", "Fokou", "Foning", "Hamadou",
        "Kameni", "Kengne", "Kouam", "Mballa", "Mbida", "Mboua", "Messi", "Moukoko", "Nana", "Ndjock",
        "Ngono", "Nguema", "Njoya", "Nkoulou", "Ntamack", "Obama", "Olinga", "Onana", "Owona", "Tchakounte",
        "Tchinda", "Tsala", "Wamba", "Yomba", "Zambo"
    };

    static readonly string[] Prenoms =
    {
        "Aïcha", "Alain", "Brice", "Carine", "Cédric", "Christelle", "Daniel", "Diane", "Éric", "Fabrice",
        "Gaëlle", "Hervé", "Ibrahim", "Josiane", "Kevin", "Laure", "Linda", "Marcel", "Nadège", "Olivier",
        "Pascaline", "Raoul", "Sandrine", "Serge", "Sylvie", "Thierry", "Ulrich", "Vanessa", "Wilfried", "Yannick"
    };

    public static BaseDonnees Creer()
    {
        var aleatoire = new Random(2026);
        var donnees = new BaseDonnees
        {
            Semestre = "Semestre 7 · 2026-2027",
            DerniereSynchronisation = DateTime.Today.AddHours(8).AddMinutes(42)
        };

        var (sel, hache) = AuthService.HacherMotDePasse(MotDePasseDemo);
        donnees.Utilisateurs.Add(new Utilisateur
        {
            NomComplet = "Marthe Etoundi",
            Email = EmailDemo,
            Role = Role.Enseignant,
            Sel = sel,
            MotDePasseHache = hache
        });

        // Promotion GL4 : les huit étudiants de la maquette puis 34 autres.
        var gl4 = new List<Etudiant>
        {
            new() { Nom = "Abena", Prenom = "Clarisse" },
            new() { Nom = "Biloa", Prenom = "Junior" },
            new() { Nom = "Djoumessi", Prenom = "Arnaud" },
            new() { Nom = "Essomba", Prenom = "Laure" },
            new() { Nom = "Fotso", Prenom = "Yvan" },
            new() { Nom = "Kamga", Prenom = "Inès" },
            new() { Nom = "Mbarga", Prenom = "Patrick" },
            new() { Nom = "Ngo Bassa", Prenom = "Estelle" }
        };
        var etudiantsMaquette = gl4.ToList();
        gl4.AddRange(Generer(aleatoire, 34, gl4.Select(e => e.Nom).ToHashSet()));
        Numeroter(gl4, "GL4", "24G", 112);

        var gl3 = Generer(aleatoire, 57, new HashSet<string>());
        Numeroter(gl3, "GL3", "25G", 104);

        donnees.Etudiants.AddRange(gl4);
        donnees.Etudiants.AddRange(gl3);

        var annee = DateTime.Today.Month >= 9 ? DateTime.Today.Year : DateTime.Today.Year - 1;

        // GLO 431 : examen final en cours de saisie.
        var mobile = new UniteEnseignement { Code = "GLO 431", Intitule = "Développement mobile", Promotion = "GL4", Credits = 4 };
        var cc = new Evaluation { Intitule = "Contrôle continu 1", Type = TypeEvaluation.Controle, Coefficient = 1, Date = new DateTime(annee, 9, 28), Terminee = true };
        var tp = new Evaluation { Intitule = "Travaux pratiques", Type = TypeEvaluation.TP, Coefficient = 1, Date = new DateTime(annee, 10, 5), Terminee = true };
        var examen = new Evaluation { Intitule = "Examen final", Type = TypeEvaluation.Examen, Coefficient = 2, Date = new DateTime(annee, 10, 12) };

        double?[,] maquette =
        {
            { 13, 15, 11.5 }, { 8.5, 12, 7 }, { 15, 16.5, 14 }, { 11, 13, null },
            { 16, 17, null }, { 9, 11, null }, { 12.5, 14, -1 }, { 14, 12.5, null }
        };
        int examensGeneres = 0;
        foreach (var etudiant in gl4)
        {
            var m = etudiant.Matricule;
            var i = etudiantsMaquette.IndexOf(etudiant);
            if (i >= 0)
            {
                cc.Notes[m] = new NoteEtudiant { Valeur = maquette[i, 0] };
                tp.Notes[m] = new NoteEtudiant { Valeur = maquette[i, 1] };
                if (maquette[i, 2] is double ex)
                    examen.Notes[m] = ex < 0 ? new NoteEtudiant { Absent = true } : new NoteEtudiant { Valeur = ex };
            }
            else
            {
                var niveau = 7 + aleatoire.NextDouble() * 9;
                cc.Notes[m] = new NoteEtudiant { Valeur = Note(aleatoire, niveau) };
                tp.Notes[m] = new NoteEtudiant { Valeur = Note(aleatoire, niveau + 1.5) };
                if (examensGeneres++ < 14)
                    examen.Notes[m] = new NoteEtudiant { Valeur = Note(aleatoire, niveau) };
            }
        }
        mobile.Evaluations.AddRange(new[] { cc, tp, examen });

        // GLO 415 : notes complètes et publiées.
        var bd = new UniteEnseignement
        {
            Code = "GLO 415", Intitule = "Bases de données avancées", Promotion = "GL4", Credits = 4,
            Publiee = true, DatePublication = new DateTime(annee, 9, 30)
        };
        var projet = new Evaluation { Intitule = "Projet", Type = TypeEvaluation.TP, Coefficient = 1, Date = new DateTime(annee, 9, 15), Terminee = true };
        var partiel = new Evaluation { Intitule = "Examen partiel", Type = TypeEvaluation.Examen, Coefficient = 2, Date = new DateTime(annee, 9, 26), Terminee = true };
        foreach (var e in gl4)
        {
            var niveau = 6 + aleatoire.NextDouble() * 10;
            projet.Notes[e.Matricule] = new NoteEtudiant { Valeur = Note(aleatoire, niveau + 1) };
            partiel.Notes[e.Matricule] = new NoteEtudiant { Valeur = Note(aleatoire, niveau) };
        }
        bd.Evaluations.AddRange(new[] { projet, partiel });

        // GLO 322 : aucune évaluation pour l'instant.
        var gl = new UniteEnseignement { Code = "GLO 322", Intitule = "Génie logiciel", Promotion = "GL3", Credits = 3 };

        donnees.Unites.AddRange(new[] { mobile, bd, gl });
        return donnees;
    }

    static List<Etudiant> Generer(Random aleatoire, int nombre, HashSet<string> exclus)
    {
        var resultat = new List<Etudiant>();
        var pris = new HashSet<string>();
        while (resultat.Count < nombre)
        {
            var nom = Noms[aleatoire.Next(Noms.Length)];
            var prenom = Prenoms[aleatoire.Next(Prenoms.Length)];
            if (exclus.Contains(nom) || !pris.Add(nom + prenom))
                continue;
            resultat.Add(new Etudiant { Nom = nom, Prenom = prenom });
        }
        return resultat;
    }

    static void Numeroter(List<Etudiant> etudiants, string promotion, string prefixe, int debut)
    {
        etudiants.Sort((a, b) => string.Compare(a.NomComplet, b.NomComplet, StringComparison.CurrentCulture));
        var numero = debut;
        foreach (var e in etudiants)
        {
            e.Promotion = promotion;
            e.Matricule = $"{prefixe}{numero:0000}";
            numero += 3 + (numero % 7);
        }
    }

    static double Note(Random aleatoire, double niveau)
    {
        var note = niveau + (aleatoire.NextDouble() * 6 - 3);
        return Math.Clamp(Math.Round(note * 2) / 2, 0, 20);
    }
}
