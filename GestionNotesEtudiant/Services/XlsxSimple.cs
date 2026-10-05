using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace GestionNotesEtudiant.Services;

/// <summary>Classeur Excel minimal (une feuille), la moyenne est écrite sous forme de formule.</summary>
public static class XlsxSimple
{
    public static byte[] Generer(TableauExport tableau)
    {
        using var flux = new MemoryStream();
        using (var archive = new ZipArchive(flux, ZipArchiveMode.Create, leaveOpen: true))
        {
            Ajouter(archive, "[Content_Types].xml",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>""");
            Ajouter(archive, "_rels/.rels",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
            Ajouter(archive, "xl/workbook.xml",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Notes" sheetId="1" r:id="rId1"/></sheets><calcPr fullCalcOnLoad="1"/></workbook>""");
            Ajouter(archive, "xl/_rels/workbook.xml.rels",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>""");
            Ajouter(archive, "xl/styles.xml",
                """<?xml version="1.0" encoding="UTF-8" standalone="yes"?><styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><numFmts count="1"><numFmt numFmtId="164" formatCode="0.00"/></numFmts><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts><fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills><borders count="1"><border/></borders><cellStyleXfs count="1"><xf/></cellStyleXfs><cellXfs count="3"><xf/><xf fontId="1" applyFont="1"/><xf numFmtId="164" applyNumberFormat="1"/></cellXfs></styleSheet>""");
            Ajouter(archive, "xl/worksheets/sheet1.xml", Feuille(tableau));
        }
        return flux.ToArray();
    }

    static string Feuille(TableauExport tableau)
    {
        var sb = new StringBuilder();
        sb.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>""");

        int ligne = 1;
        Ligne(sb, ligne++, new[] { tableau.Titre }, style: 1);
        Ligne(sb, ligne++, new[] { tableau.SousTitre });
        ligne++;
        Ligne(sb, ligne++, tableau.Colonnes.ToArray(), style: 1);

        foreach (var cellules in tableau.Lignes)
        {
            sb.Append($"<row r=\"{ligne}\">");
            for (int c = 0; c < cellules.Length; c++)
            {
                var reference = Colonne(c) + ligne;
                if (c == tableau.ColonneMoyenne && tableau.ColonnesNotes.Count > 0)
                    sb.Append(Formule(reference, ligne, tableau, cellules[c]));
                else
                    sb.Append(Cellule(reference, cellules[c]));
            }
            sb.Append("</row>");
            ligne++;
        }

        if (tableau.Statistiques.Count > 0)
        {
            ligne++;
            foreach (var (libelle, valeur) in tableau.Statistiques)
                Ligne(sb, ligne++, new[] { libelle, valeur });
        }

        sb.Append("</sheetData></worksheet>");
        return sb.ToString();
    }

    static void Ligne(StringBuilder sb, int ligne, string[] cellules, int style = 0)
    {
        sb.Append($"<row r=\"{ligne}\">");
        for (int c = 0; c < cellules.Length; c++)
            sb.Append(Cellule(Colonne(c) + ligne, cellules[c], style));
        sb.Append("</row>");
    }

    static string Cellule(string reference, string valeur, int style = 0)
    {
        if (string.IsNullOrEmpty(valeur))
            return string.Empty;
        if (style == 0 && Calculs.TryLireNote(valeur, out var nombre) && !valeur.Contains('/') && char.IsDigit(valeur[0]) && !reference.StartsWith('A'))
            return $"<c r=\"{reference}\"><v>{nombre.ToString(CultureInfo.InvariantCulture)}</v></c>";
        return $"<c r=\"{reference}\" t=\"inlineStr\"{(style > 0 ? $" s=\"{style}\"" : "")}><is><t>{SecurityElement.Escape(valeur)}</t></is></c>";
    }

    static string Formule(string reference, int ligne, TableauExport tableau, string valeurCalculee)
    {
        var debut = Colonne(tableau.ColonnesNotes[0].Colonne) + ligne;
        var fin = Colonne(tableau.ColonnesNotes[^1].Colonne) + ligne;
        var coefficients = string.Join(',', tableau.ColonnesNotes.Select(n => n.Coefficient));
        var formule = $"IF(COUNTIF({debut}:{fin},\"ABS\")>0,\"\",IFERROR(ROUND(SUMPRODUCT({debut}:{fin},{{{coefficients}}})" +
                      $"/SUMPRODUCT(--ISNUMBER({debut}:{fin}),{{{coefficients}}}),2),\"\"))";
        var cache = Calculs.TryLireNote(valeurCalculee, out var v) ? $"<v>{v.ToString(CultureInfo.InvariantCulture)}</v>" : string.Empty;
        return $"<c r=\"{reference}\" s=\"2\"><f>{SecurityElement.Escape(formule)}</f>{cache}</c>";
    }

    static string Colonne(int index)
    {
        var nom = string.Empty;
        index++;
        while (index > 0)
        {
            var reste = (index - 1) % 26;
            nom = (char)('A' + reste) + nom;
            index = (index - 1) / 26;
        }
        return nom;
    }

    static void Ajouter(ZipArchive archive, string nom, string contenu)
    {
        var entree = archive.CreateEntry(nom, CompressionLevel.Optimal);
        using var ecrivain = new StreamWriter(entree.Open(), new UTF8Encoding(false));
        ecrivain.Write(contenu);
    }
}
