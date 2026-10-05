using Microsoft.Maui.Controls.Shapes;

namespace GestionNotesEtudiant.Controls;

/// <summary>
/// Champ de saisie de la charte : états normal, actif (focus), erreur et désactivé.
/// </summary>
public class ChampSaisie : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(ChampSaisie), string.Empty, BindingMode.TwoWay,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._entree.Text = (string?)n);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(ChampSaisie), string.Empty,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._entree.Placeholder = (string?)n);

    public static readonly BindableProperty MessageErreurProperty = BindableProperty.Create(
        nameof(MessageErreur), typeof(string), typeof(ChampSaisie), null,
        propertyChanged: (b, _, _) => ((ChampSaisie)b).MettreAJour());

    public static readonly BindableProperty EnErreurProperty = BindableProperty.Create(
        nameof(EnErreur), typeof(bool), typeof(ChampSaisie), false,
        propertyChanged: (b, _, _) => ((ChampSaisie)b).MettreAJour());

    public static readonly BindableProperty EstMotDePasseProperty = BindableProperty.Create(
        nameof(EstMotDePasse), typeof(bool), typeof(ChampSaisie), false,
        propertyChanged: (b, _, _) => ((ChampSaisie)b).MettreAJour());

    public static readonly BindableProperty EstGriseProperty = BindableProperty.Create(
        nameof(EstGrise), typeof(bool), typeof(ChampSaisie), false,
        propertyChanged: (b, _, _) => ((ChampSaisie)b).MettreAJour());

    public static readonly BindableProperty ClavierProperty = BindableProperty.Create(
        nameof(Clavier), typeof(Keyboard), typeof(ChampSaisie), Keyboard.Default,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._entree.Keyboard = (Keyboard)n);

    public static readonly BindableProperty CentreProperty = BindableProperty.Create(
        nameof(Centre), typeof(bool), typeof(ChampSaisie), false,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._entree.HorizontalTextAlignment = (bool)n ? TextAlignment.Center : TextAlignment.Start);

    public static readonly BindableProperty HauteurProperty = BindableProperty.Create(
        nameof(Hauteur), typeof(double), typeof(ChampSaisie), 50d,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._cadre.HeightRequest = (double)n);

    public static readonly BindableProperty TailleTexteProperty = BindableProperty.Create(
        nameof(TailleTexte), typeof(double), typeof(ChampSaisie), 16d,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._entree.FontSize = (double)n);

    public static readonly BindableProperty LongueurMaxProperty = BindableProperty.Create(
        nameof(LongueurMax), typeof(int), typeof(ChampSaisie), int.MaxValue,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._entree.MaxLength = (int)n);

    public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(
        nameof(ReturnType), typeof(ReturnType), typeof(ChampSaisie), ReturnType.Default,
        propertyChanged: (b, _, n) => ((ChampSaisie)b)._entree.ReturnType = (ReturnType)n);

    readonly Entry _entree;
    readonly Border _cadre;
    readonly ImageButton _oeil;
    readonly HorizontalStackLayout _ligneErreur;
    readonly Label _texteErreur;
    bool _masque = true;

    public event EventHandler? Valide;
    public event EventHandler? FocusPerdu;

    public ChampSaisie()
    {
        _entree = new Entry
        {
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(12, 0, 4, 0),
            FontSize = 16
        };
        _entree.TextChanged += (_, e) => Text = e.NewTextValue;
        _entree.Focused += (_, _) => MettreAJour();
        _entree.Unfocused += (_, _) =>
        {
            MettreAJour();
            FocusPerdu?.Invoke(this, EventArgs.Empty);
        };
        _entree.Completed += (_, _) => Valide?.Invoke(this, EventArgs.Empty);

        _oeil = new ImageButton
        {
            Source = "ic_oeil.png",
            WidthRequest = 44,
            HeightRequest = 44,
            Padding = 11,
            BackgroundColor = Colors.Transparent,
            IsVisible = false,
            Margin = new Thickness(0, 0, 4, 0)
        };
        SemanticProperties.SetDescription(_oeil, "Afficher le mot de passe");
        _oeil.Clicked += (_, _) =>
        {
            _masque = !_masque;
            MettreAJour();
        };

        var grille = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        grille.Add(_entree, 0);
        grille.Add(_oeil, 1);

        _cadre = new Border
        {
            HeightRequest = 50,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Content = grille
        };

        _texteErreur = new Label { FontSize = 13, TextColor = Theme.Erreur, VerticalOptions = LayoutOptions.Center };
        _ligneErreur = new HorizontalStackLayout
        {
            Spacing = 6,
            Margin = new Thickness(0, 6, 0, 0),
            IsVisible = false,
            Children =
            {
                new Image { Source = "ic_alerte_erreur.png", WidthRequest = 15, HeightRequest = 15, VerticalOptions = LayoutOptions.Center },
                _texteErreur
            }
        };

        Content = new VerticalStackLayout { Children = { _cadre, _ligneErreur } };
        MettreAJour();
    }

    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string? Placeholder { get => (string?)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string? MessageErreur { get => (string?)GetValue(MessageErreurProperty); set => SetValue(MessageErreurProperty, value); }
    public bool EnErreur { get => (bool)GetValue(EnErreurProperty); set => SetValue(EnErreurProperty, value); }
    public bool EstMotDePasse { get => (bool)GetValue(EstMotDePasseProperty); set => SetValue(EstMotDePasseProperty, value); }
    public bool EstGrise { get => (bool)GetValue(EstGriseProperty); set => SetValue(EstGriseProperty, value); }
    public Keyboard Clavier { get => (Keyboard)GetValue(ClavierProperty); set => SetValue(ClavierProperty, value); }
    public bool Centre { get => (bool)GetValue(CentreProperty); set => SetValue(CentreProperty, value); }
    public double Hauteur { get => (double)GetValue(HauteurProperty); set => SetValue(HauteurProperty, value); }
    public double TailleTexte { get => (double)GetValue(TailleTexteProperty); set => SetValue(TailleTexteProperty, value); }
    public int LongueurMax { get => (int)GetValue(LongueurMaxProperty); set => SetValue(LongueurMaxProperty, value); }
    public ReturnType ReturnType { get => (ReturnType)GetValue(ReturnTypeProperty); set => SetValue(ReturnTypeProperty, value); }

    public bool EstActif => _entree.IsFocused;

    public new void Focus() => _entree.Focus();

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            MettreAJour();
    }

    void MettreAJour()
    {
        var erreur = EnErreur || !string.IsNullOrEmpty(MessageErreur);
        var actif = _entree.IsFocused;

        _entree.IsPassword = EstMotDePasse && _masque;
        _entree.IsEnabled = IsEnabled;
        _oeil.IsVisible = EstMotDePasse;
        _oeil.Source = _masque ? "ic_oeil.png" : "ic_oeil_barre.png";

        if (!IsEnabled)
        {
            _cadre.BackgroundColor = Theme.Desactive;
            _cadre.Stroke = Theme.Desactive;
            _cadre.StrokeThickness = 1;
            _entree.TextColor = Theme.TexteDesactive;
        }
        else if (erreur)
        {
            _cadre.BackgroundColor = Theme.Surface;
            _cadre.Stroke = Theme.Erreur;
            _cadre.StrokeThickness = 2;
            _entree.TextColor = Theme.Encre;
        }
        else if (actif)
        {
            _cadre.BackgroundColor = Theme.Surface;
            _cadre.Stroke = Theme.Primaire;
            _cadre.StrokeThickness = 2;
            _entree.TextColor = Theme.Encre;
        }
        else if (EstGrise)
        {
            _cadre.BackgroundColor = Theme.Desactive;
            _cadre.Stroke = Theme.Desactive;
            _cadre.StrokeThickness = 1;
            _entree.TextColor = Theme.TexteSecondaire;
        }
        else
        {
            _cadre.BackgroundColor = Theme.Surface;
            _cadre.Stroke = Theme.Bordure;
            _cadre.StrokeThickness = 1;
            _entree.TextColor = Theme.Encre;
        }

        _texteErreur.Text = MessageErreur;
        _ligneErreur.IsVisible = !string.IsNullOrEmpty(MessageErreur);
    }
}
