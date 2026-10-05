using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class ExportViewModel : VueModeleBase, IQueryAttributable
{
    readonly DonneesService _donnees;
    readonly ExportService _export;
    string _code = string.Empty;

    public ExportViewModel(DonneesService donnees, ExportService export)
    {
        _donnees = donnees;
        _export = export;
    }

    [ObservableProperty] string sousTitre = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PdfSelectionne), nameof(CsvSelectionne), nameof(ExcelSelectionne), nameof(TexteBouton))]
    FormatExport format = FormatExport.Pdf;

    [ObservableProperty] bool notesParEvaluation = true;
    [ObservableProperty] bool moyennes = true;
    [ObservableProperty] bool statistiques;
    [ObservableProperty] bool anonymiser;

    [ObservableProperty] string taillePdf = string.Empty;
    [ObservableProperty] string tailleCsv = string.Empty;
    [ObservableProperty] string tailleExcel = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExporterCommand))]
    bool contenuValide = true;

    public bool PdfSelectionne => Format == FormatExport.Pdf;
    public bool CsvSelectionne => Format == FormatExport.Csv;
    public bool ExcelSelectionne => Format == FormatExport.Excel;

    public string TexteBouton => Format switch
    {
        FormatExport.Pdf => "Exporter le PDF",
        FormatExport.Csv => "Exporter le CSV",
        _ => "Exporter le fichier Excel"
    };

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("code", out var c) && c is string code)
            _code = code;
        var ue = _donnees.Unite(_code);
        if (ue is not null)
            SousTitre = $"{ue.Code} · {ue.Intitule}";
        Estimer();
    }

    OptionsExport Options => new(NotesParEvaluation, Moyennes, Statistiques, Anonymiser);

    partial void OnNotesParEvaluationChanged(bool value) => Estimer();
    partial void OnMoyennesChanged(bool value) => Estimer();
    partial void OnStatistiquesChanged(bool value) => Estimer();
    partial void OnAnonymiserChanged(bool value) => Estimer();

    /// <summary>Taille réelle du fichier qui sera produit, pour un choix éclairé.</summary>
    void Estimer()
    {
        var ue = _donnees.Unite(_code);
        ContenuValide = NotesParEvaluation || Moyennes || Statistiques;
        if (ue is null)
            return;

        var tableau = _export.Construire(ue, Options);
        TaillePdf = Taille(_export.Generer(tableau, FormatExport.Pdf).Length);
        TailleCsv = Taille(_export.Generer(tableau, FormatExport.Csv).Length);
        TailleExcel = Taille(_export.Generer(tableau, FormatExport.Excel).Length);
    }

    static string Taille(int octets) => $"≈ {Math.Max(1, (int)Math.Ceiling(octets / 1024d))} Ko";

    [RelayCommand]
    void Choisir(string format) => Format = Enum.Parse<FormatExport>(format);

    [RelayCommand(CanExecute = nameof(ContenuValide))]
    async Task Exporter()
    {
        var ue = _donnees.Unite(_code);
        if (ue is null)
            return;

        EstOccupe = true;
        try
        {
            var tableau = _export.Construire(ue, Options);
            var chemin = await _export.EcrireFichierAsync(ue, tableau, Format);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Notes {ue.Code}",
                File = new ShareFile(chemin, ExportService.TypeMime(Format))
            });
        }
        catch (Exception ex)
        {
            await Alerte("Export impossible", ex.Message);
        }
        finally
        {
            EstOccupe = false;
        }
    }
}
