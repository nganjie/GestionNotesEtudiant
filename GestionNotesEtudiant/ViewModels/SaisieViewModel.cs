using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionNotesEtudiant.Models;
using GestionNotesEtudiant.Services;

namespace GestionNotesEtudiant.ViewModels;

public partial class LigneSaisie : ObservableObject
{
    readonly double _noteSur;
    readonly Action _surModification;

    public LigneSaisie(Etudiant etudiant, NoteEtudiant? note, double noteSur, Action surModification)
    {
        Matricule = etudiant.Matricule;
        Nom = etudiant.NomComplet;
        _noteSur = noteSur;
        _surModification = surModification;
        texte = note switch
        {
            { Absent: true } => "ABS",
            { Valeur: double v } => Calculs.FormatNote(v),
            _ => string.Empty
        };
    }

    public string Matricule { get; }
    public string Nom { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MessageErreur), nameof(EstAbsent), nameof(EstSaisie))]
    string texte;

    partial void OnTexteChanged(string value) => _surModification();

    public bool EstAbsent => Texte.Trim().Equals("ABS", StringComparison.OrdinalIgnoreCase)
                             || Texte.Trim().Equals("A", StringComparison.OrdinalIgnoreCase);

    public bool EstSaisie => !string.IsNullOrWhiteSpace(Texte) && MessageErreur is null;

    public string? MessageErreur
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Texte) || EstAbsent)
                return null;
            if (!Calculs.TryLireNote(Texte, out var valeur))
                return "Saisissez une note ou ABS pour un absent";
            if (valeur < 0 || valeur > _noteSur)
                return $"La note doit être comprise entre 0 et {Calculs.FormatNote(_noteSur)}";
            return null;
        }
    }

    /// <summary>Appelé quand le champ perd le focus : « a » devient « ABS », « 12.5 » devient « 12,5 ».</summary>
    public void Normaliser()
    {
        if (EstAbsent)
            Texte = "ABS";
        else if (MessageErreur is null && Calculs.TryLireNote(Texte, out var valeur))
            Texte = Calculs.FormatNote(valeur);
    }

    public NoteEtudiant? VersNote()
    {
        if (EstAbsent)
            return new NoteEtudiant { Absent = true };
        if (MessageErreur is null && Calculs.TryLireNote(Texte, out var valeur))
            return new NoteEtudiant { Valeur = valeur };
        return null;
    }
}

public partial class SaisieViewModel : VueModeleBase, IQueryAttributable
{
    readonly DonneesService _donnees;
    string _code = string.Empty;
    string _evaluationId = string.Empty;
    bool _chargement;

    public SaisieViewModel(DonneesService donnees) => _donnees = donnees;

    public ObservableCollection<LigneSaisie> Lignes { get; } = new();

    [ObservableProperty] string titre = string.Empty;
    [ObservableProperty] string sousTitre = string.Empty;
    [ObservableProperty] string compteur = string.Empty;
    [ObservableProperty] double progression;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EtatEnregistrement))]
    [NotifyCanExecuteChangedFor(nameof(EnregistrerCommand))]
    bool modifie;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TerminerCommand))]
    bool peutTerminer;

    public string EtatEnregistrement => Modifie ? "Modifications non enregistrées" : "Enregistré sur l'appareil";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("code", out var c) && c is string code)
            _code = code;
        if (query.TryGetValue("evaluation", out var e) && e is string id)
            _evaluationId = id;
        Charger();
    }

    (UniteEnseignement Ue, Evaluation Evaluation)? Trouver()
    {
        var ue = _donnees.Unite(_code);
        var evaluation = ue?.Evaluations.FirstOrDefault(x => x.Id == _evaluationId);
        return ue is null || evaluation is null ? null : (ue, evaluation);
    }

    void Charger()
    {
        if (Trouver() is not var (ue, evaluation))
            return;

        _chargement = true;
        Titre = evaluation.Intitule;
        SousTitre = $"{ue.Code} · coef. {evaluation.Coefficient} · noté sur {Calculs.FormatNote(evaluation.NoteSur)}";
        Lignes.Clear();
        foreach (var etudiant in _donnees.EtudiantsDe(ue.Promotion))
            Lignes.Add(new LigneSaisie(etudiant, evaluation.NoteDe(etudiant.Matricule), evaluation.NoteSur, () => SurModification()));
        _chargement = false;
        Modifie = false;
        Recompter();
    }

    void SurModification()
    {
        if (_chargement)
            return;
        Modifie = true;
        Recompter();
    }

    void Recompter()
    {
        var saisies = Lignes.Count(l => l.EstSaisie);
        Compteur = $"{saisies} / {Lignes.Count} saisies";
        Progression = Lignes.Count == 0 ? 0 : (double)saisies / Lignes.Count;
        PeutTerminer = Lignes.Count > 0 && saisies == Lignes.Count;
    }

    async Task<bool> SauvegarderAsync()
    {
        if (Trouver() is not var (ue, evaluation))
            return false;

        if (Lignes.Any(l => l.MessageErreur is not null))
        {
            await Alerte("Notes à corriger", "Corrigez les notes signalées en rouge avant d'enregistrer.");
            return false;
        }

        foreach (var ligne in Lignes)
        {
            if (ligne.VersNote() is { } note)
                evaluation.Notes[ligne.Matricule] = note;
            else
                evaluation.Notes.Remove(ligne.Matricule);
        }
        evaluation.Terminee = PeutTerminer;
        await _donnees.EnregistrerAsync();
        Modifie = false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(Modifie))]
    async Task Enregistrer() => await SauvegarderAsync();

    [RelayCommand(CanExecute = nameof(PeutTerminer))]
    async Task Terminer()
    {
        if (await SauvegarderAsync())
            await Retour();
    }

    [RelayCommand]
    async Task Quitter()
    {
        if (Modifie)
        {
            var choix = await Shell.Current.DisplayActionSheetAsync(
                "Des notes ne sont pas enregistrées.", "Rester", null, "Enregistrer et quitter", "Quitter sans enregistrer");
            if (choix == "Enregistrer et quitter")
            {
                if (!await SauvegarderAsync())
                    return;
            }
            else if (choix != "Quitter sans enregistrer")
            {
                return;
            }
        }
        Modifie = false;
        await Retour();
    }
}
