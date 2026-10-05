using System.Globalization;
using System.Text;

namespace GestionNotesEtudiant.Services;

/// <summary>
/// Générateur PDF minimal (texte et traits uniquement, polices standard non embarquées)
/// pour produire un relevé léger sans bibliothèque externe.
/// </summary>
public static class PdfSimple
{
    const double Largeur = 595, Hauteur = 842, Marge = 40, HauteurLigne = 20;

    public static byte[] Generer(TableauExport tableau)
    {
        var pages = new List<StringBuilder>();
        var largeurs = CalculerLargeurs(tableau);

        StringBuilder page = NouvellePage(pages);
        double y = Hauteur - Marge;

        Texte(page, Marge, y - 16, 16, true, "0.078 0.102 0.165", tableau.Titre);
        Texte(page, Marge, y - 34, 10, false, "0.337 0.376 0.455", tableau.SousTitre);
        y -= 60;

        void EnTete()
        {
            Rectangle(page, Marge, y - HauteurLigne, Largeur - 2 * Marge, HauteurLigne, "0.910 0.925 0.969");
            Ligne(page, tableau.Colonnes.ToArray(), largeurs, y, true);
            y -= HauteurLigne;
        }

        EnTete();
        foreach (var ligne in tableau.Lignes)
        {
            if (y - HauteurLigne < Marge + 20)
            {
                page = NouvellePage(pages);
                y = Hauteur - Marge;
                EnTete();
            }
            Ligne(page, ligne, largeurs, y, false);
            y -= HauteurLigne;
            Trait(page, Marge, y, Largeur - Marge, y);
        }

        if (tableau.Statistiques.Count > 0)
        {
            if (y - 30 - tableau.Statistiques.Count * 16 < Marge)
            {
                page = NouvellePage(pages);
                y = Hauteur - Marge;
            }
            y -= 28;
            Texte(page, Marge, y, 12, true, "0.078 0.102 0.165", "Statistiques de la promotion");
            foreach (var (libelle, valeur) in tableau.Statistiques)
            {
                y -= 16;
                Texte(page, Marge, y, 10, false, "0.337 0.376 0.455", libelle);
                Texte(page, Marge + 160, y, 10, true, "0.078 0.102 0.165", valeur);
            }
        }

        for (int i = 0; i < pages.Count; i++)
            Texte(pages[i], Largeur - Marge - 60, 24, 8, false, "0.337 0.376 0.455", $"Page {i + 1} / {pages.Count}");

        return Assembler(pages);
    }

    static StringBuilder NouvellePage(List<StringBuilder> pages)
    {
        var page = new StringBuilder();
        pages.Add(page);
        return page;
    }

    static double[] CalculerLargeurs(TableauExport tableau)
    {
        var poids = tableau.Colonnes.Select(c => c switch
        {
            "Nom et prénom" => 3.2,
            "Matricule" => 1.4,
            "Décision" => 1.5,
            _ => 1.1
        }).ToArray();
        var total = poids.Sum();
        return poids.Select(p => p / total * (Largeur - 2 * Marge)).ToArray();
    }

    static void Ligne(StringBuilder page, string[] cellules, double[] largeurs, double y, bool gras)
    {
        double x = Marge;
        for (int i = 0; i < cellules.Length && i < largeurs.Length; i++)
        {
            var taille = 9;
            var texte = Tronquer(cellules[i], largeurs[i] - 8, taille);
            Texte(page, x + 4, y - 13.5, taille, gras, "0.078 0.102 0.165", texte);
            x += largeurs[i];
        }
    }

    static string Tronquer(string texte, double largeurMax, double taille)
    {
        var maxCaracteres = (int)(largeurMax / (taille * 0.52));
        return texte.Length <= maxCaracteres ? texte : texte[..Math.Max(1, maxCaracteres - 1)] + "…";
    }

    static void Texte(StringBuilder page, double x, double y, double taille, bool gras, string couleur, string texte) =>
        page.Append($"{couleur} rg BT /{(gras ? "F2" : "F1")} {N(taille)} Tf {N(x)} {N(y)} Td ({Echapper(texte)}) Tj ET\n");

    static void Rectangle(StringBuilder page, double x, double y, double l, double h, string couleur) =>
        page.Append($"{couleur} rg {N(x)} {N(y)} {N(l)} {N(h)} re f\n");

    static void Trait(StringBuilder page, double x1, double y1, double x2, double y2) =>
        page.Append($"0.851 0.867 0.898 RG 0.5 w {N(x1)} {N(y1)} m {N(x2)} {N(y2)} l S\n");

    static string N(double valeur) => valeur.ToString("0.##", CultureInfo.InvariantCulture);

    static string Echapper(string texte)
    {
        var sb = new StringBuilder();
        foreach (var c in texte.Replace("≥", ">="))
        {
            if (c is '(' or ')' or '\\')
                sb.Append('\\').Append(c);
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>Conversion vers WinAnsiEncoding (accents français, tirets, points médians).</summary>
    static byte[] WinAnsi(string texte)
    {
        var octets = new byte[texte.Length];
        for (int i = 0; i < texte.Length; i++)
        {
            var c = texte[i];
            octets[i] = c switch
            {
                '—' => 0x97,
                '–' => 0x96,
                '’' => 0x92,
                '…' => 0x85,
                '€' => 0x80,
                ' ' or ' ' => 0x20,
                _ when c < 0x80 || (c >= 0xA0 && c <= 0xFF) => (byte)c,
                _ => (byte)'?'
            };
        }
        return octets;
    }

    static byte[] Assembler(List<StringBuilder> pages)
    {
        using var flux = new MemoryStream();
        var positions = new List<long>();

        void Ecrire(string s)
        {
            var octets = WinAnsi(s);
            flux.Write(octets, 0, octets.Length);
        }

        void Objet(string contenu)
        {
            positions.Add(flux.Position);
            Ecrire($"{positions.Count} 0 obj\n{contenu}\nendobj\n");
        }

        Ecrire("%PDF-1.4\n");
        flux.Write(new byte[] { (byte)'%', 0xE2, 0xE3, 0xCF, 0xD3, (byte)'\n' });

        // 1 : catalogue, 2 : arbre des pages, 3-4 : polices, puis page + contenu par page.
        var kids = string.Join(' ', Enumerable.Range(0, pages.Count).Select(i => $"{5 + i * 2} 0 R"));
        Objet("<< /Type /Catalog /Pages 2 0 R >>");
        Objet($"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>");
        Objet("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        Objet("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

        for (int i = 0; i < pages.Count; i++)
        {
            var contenu = pages[i].ToString();
            Objet($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {N(Largeur)} {N(Hauteur)}] " +
                  $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {6 + i * 2} 0 R >>");
            Objet($"<< /Length {WinAnsi(contenu).Length} >>\nstream\n{contenu}endstream");
        }

        var debutXref = flux.Position;
        var xref = new StringBuilder();
        xref.Append($"xref\n0 {positions.Count + 1}\n0000000000 65535 f \n");
        foreach (var position in positions)
            xref.Append($"{position:0000000000} 00000 n \n");
        xref.Append($"trailer\n<< /Size {positions.Count + 1} /Root 1 0 R >>\nstartxref\n{debutXref}\n%%EOF\n");
        Ecrire(xref.ToString());

        return flux.ToArray();
    }
}
