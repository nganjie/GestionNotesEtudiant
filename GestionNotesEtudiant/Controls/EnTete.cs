namespace GestionNotesEtudiant.Controls;

/// <summary>Barre de titre avec bouton retour, titre, sous-titre et zone d'action à droite.</summary>
public class EnTete : ContentView
{
    public static readonly BindableProperty TitreProperty = BindableProperty.Create(
        nameof(Titre), typeof(string), typeof(EnTete), string.Empty,
        propertyChanged: (b, _, n) => ((EnTete)b)._titre.Text = (string?)n);

    public static readonly BindableProperty SousTitreProperty = BindableProperty.Create(
        nameof(SousTitre), typeof(string), typeof(EnTete), string.Empty,
        propertyChanged: (b, _, n) =>
        {
            var e = (EnTete)b;
            e._sousTitre.Text = (string?)n;
            e._sousTitre.IsVisible = !string.IsNullOrEmpty((string?)n);
        });

    public static readonly BindableProperty ActionProperty = BindableProperty.Create(
        nameof(Action), typeof(View), typeof(EnTete), null,
        propertyChanged: (b, o, n) =>
        {
            var e = (EnTete)b;
            if (o is View ancien)
                e._grille.Remove(ancien);
            if (n is View nouveau)
                e._grille.Add(nouveau, 2);
        });

    readonly Grid _grille;
    readonly Label _titre;
    readonly Label _sousTitre;

    public event EventHandler? Retour;

    public EnTete()
    {
        var retour = new ImageButton
        {
            Source = "ic_retour.png",
            WidthRequest = 44,
            HeightRequest = 44,
            Padding = 10,
            Margin = new Thickness(-10, 0, 0, 0),
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Start
        };
        SemanticProperties.SetDescription(retour, "Retour");
        retour.Clicked += async (_, _) =>
        {
            if (Retour is not null)
                Retour.Invoke(this, EventArgs.Empty);
            else
                await Shell.Current.GoToAsync("..");
        };

        _titre = new Label { FontFamily = Theme.PoliceDemiGras, FontSize = 20, LineBreakMode = LineBreakMode.TailTruncation };
        _sousTitre = new Label { FontSize = 13, TextColor = Theme.TexteSecondaire, IsVisible = false };

        _grille = new Grid
        {
            ColumnSpacing = 6,
            Padding = new Thickness(16, 12, 16, 0),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        _grille.Add(retour, 0);
        _grille.Add(new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 2,
            Children = { _titre, _sousTitre }
        }, 1);
        Content = _grille;
    }

    public string Titre { get => (string)GetValue(TitreProperty); set => SetValue(TitreProperty, value); }
    public string SousTitre { get => (string)GetValue(SousTitreProperty); set => SetValue(SousTitreProperty, value); }
    public View? Action { get => (View?)GetValue(ActionProperty); set => SetValue(ActionProperty, value); }
}
