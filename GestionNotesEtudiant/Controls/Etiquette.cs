using Microsoft.Maui.Controls.Shapes;

namespace GestionNotesEtudiant.Controls;

public enum VarianteEtiquette
{
    Neutre,
    Succes,
    Attention
}

/// <summary>Étiquette d'état : Publié, En cours, Brouillon, Complet, NV, ABS.</summary>
public class Etiquette : ContentView
{
    public static readonly BindableProperty TexteProperty = BindableProperty.Create(
        nameof(Texte), typeof(string), typeof(Etiquette), string.Empty,
        propertyChanged: (b, _, _) => ((Etiquette)b).MettreAJour());

    public static readonly BindableProperty VarianteProperty = BindableProperty.Create(
        nameof(Variante), typeof(VarianteEtiquette), typeof(Etiquette), VarianteEtiquette.Neutre,
        propertyChanged: (b, _, _) => ((Etiquette)b).MettreAJour());

    readonly Border _fond;
    readonly Label _texte;

    public Etiquette()
    {
        _texte = new Label { FontSize = 13, FontFamily = Theme.PoliceDemiGras };
        _fond = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Padding = new Thickness(8, 3),
            Content = _texte
        };
        Content = _fond;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        MettreAJour();
    }

    public string Texte { get => (string)GetValue(TexteProperty); set => SetValue(TexteProperty, value); }
    public VarianteEtiquette Variante { get => (VarianteEtiquette)GetValue(VarianteProperty); set => SetValue(VarianteProperty, value); }

    void MettreAJour()
    {
        _texte.Text = Texte;
        (_fond.BackgroundColor, _texte.TextColor) = Variante switch
        {
            VarianteEtiquette.Succes => (Theme.SuccesClair, Theme.Succes),
            VarianteEtiquette.Attention => (Theme.AttentionClair, Theme.Attention),
            _ => (Theme.Desactive, Theme.TexteSecondaire)
        };
        IsVisible = !string.IsNullOrEmpty(Texte);
    }
}
