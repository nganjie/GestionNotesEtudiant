using Microsoft.Maui.Controls.Shapes;

namespace GestionNotesEtudiant.Controls;

/// <summary>Sélecteur segmenté (Enseignant / Étudiant, Contrôle / TP / Examen).</summary>
public class Segmente : ContentView
{
    public static readonly BindableProperty OptionsProperty = BindableProperty.Create(
        nameof(Options), typeof(IList<string>), typeof(Segmente), null,
        propertyChanged: (b, _, _) => ((Segmente)b).Construire());

    public static readonly BindableProperty IndexSelectionneProperty = BindableProperty.Create(
        nameof(IndexSelectionne), typeof(int), typeof(Segmente), 0, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((Segmente)b).MettreAJour());

    readonly Grid _grille = new() { ColumnSpacing = 4 };
    readonly List<(Border Fond, Label Texte)> _segments = new();

    public Segmente()
    {
        Content = new Border
        {
            BackgroundColor = Color.FromArgb("#E8EBF0"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = 4,
            HeightRequest = 48,
            Content = _grille
        };
    }

    public IList<string>? Options { get => (IList<string>?)GetValue(OptionsProperty); set => SetValue(OptionsProperty, value); }
    public int IndexSelectionne { get => (int)GetValue(IndexSelectionneProperty); set => SetValue(IndexSelectionneProperty, value); }

    void Construire()
    {
        _grille.Children.Clear();
        _grille.ColumnDefinitions.Clear();
        _segments.Clear();
        if (Options is null)
            return;

        for (int i = 0; i < Options.Count; i++)
        {
            var index = i;
            var texte = new Label
            {
                Text = Options[i],
                FontSize = 15,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            var fond = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Content = texte
            };
            SemanticProperties.SetDescription(fond, Options[i]);
            fond.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => IndexSelectionne = index) });

            _grille.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _grille.Add(fond, i);
            _segments.Add((fond, texte));
        }
        MettreAJour();
    }

    void MettreAJour()
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            var (fond, texte) = _segments[i];
            var selectionne = i == IndexSelectionne;
            fond.BackgroundColor = selectionne ? Theme.Surface : Colors.Transparent;
            texte.FontFamily = selectionne ? Theme.PoliceDemiGras : Theme.PoliceNormale;
            texte.TextColor = selectionne ? Theme.Encre : Theme.TexteSecondaire;
        }
    }
}
