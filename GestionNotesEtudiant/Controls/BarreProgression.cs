using Microsoft.Maui.Controls.Shapes;

namespace GestionNotesEtudiant.Controls;

/// <summary>Barre de progression épaisse et arrondie, sans animation.</summary>
public class BarreProgression : ContentView
{
    public static readonly BindableProperty ValeurProperty = BindableProperty.Create(
        nameof(Valeur), typeof(double), typeof(BarreProgression), 0d,
        propertyChanged: (b, _, _) => ((BarreProgression)b).MettreAJour());

    public static readonly BindableProperty CouleurFondProperty = BindableProperty.Create(
        nameof(CouleurFond), typeof(Color), typeof(BarreProgression), Color.FromArgb("#E4E7EC"),
        propertyChanged: (b, _, n) => ((BarreProgression)b)._fond.BackgroundColor = (Color)n);

    readonly Border _fond;
    readonly Border _barre;

    public BarreProgression()
    {
        _barre = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            BackgroundColor = Theme.Primaire,
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = 0
        };
        _fond = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            BackgroundColor = Color.FromArgb("#E4E7EC"),
            HeightRequest = 6,
            Content = _barre
        };
        Content = _fond;
        SizeChanged += (_, _) => MettreAJour();
    }

    public double Valeur { get => (double)GetValue(ValeurProperty); set => SetValue(ValeurProperty, value); }
    public Color CouleurFond { get => (Color)GetValue(CouleurFondProperty); set => SetValue(CouleurFondProperty, value); }

    void MettreAJour()
    {
        if (Width > 0)
            _barre.WidthRequest = Width * Math.Clamp(Valeur, 0, 1);
    }
}
