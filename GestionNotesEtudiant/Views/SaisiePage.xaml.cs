using GestionNotesEtudiant.Controls;
using GestionNotesEtudiant.ViewModels;

namespace GestionNotesEtudiant.Views;

public partial class SaisiePage : ContentPage
{
    readonly SaisieViewModel _vueModele;

    public SaisiePage(SaisieViewModel vueModele)
    {
        InitializeComponent();
        BindingContext = _vueModele = vueModele;
        Entete.Retour += (_, _) => _vueModele.QuitterCommand.Execute(null);
    }

    void OnNoteQuittee(object? sender, EventArgs e)
    {
        if (sender is ChampSaisie { BindingContext: LigneSaisie ligne })
            ligne.Normaliser();
    }

    async void OnNomTouche(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: LigneSaisie ligne })
            return;

        var choix = await DisplayActionSheetAsync(ligne.Nom, "Annuler", null,
            ligne.EstAbsent ? "Retirer l'absence" : "Marquer absent", "Effacer la note");
        if (choix == "Marquer absent")
            ligne.Texte = "ABS";
        else if (choix is "Retirer l'absence" or "Effacer la note")
            ligne.Texte = string.Empty;
    }

    protected override bool OnBackButtonPressed()
    {
        _vueModele.QuitterCommand.Execute(null);
        return true;
    }
}
