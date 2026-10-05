namespace GestionNotesEtudiant.Controls;

/// <summary>Accès typé aux couleurs de la charte définies dans Colors.xaml.</summary>
public static class Theme
{
    public static Color Couleur(string cle) =>
        Application.Current?.Resources.TryGetValue(cle, out var valeur) == true && valeur is Color couleur
            ? couleur
            : Colors.Transparent;

    public static Color Encre => Couleur("Encre");
    public static Color Primaire => Couleur("Primaire");
    public static Color PrimaireClair => Couleur("PrimaireClair");
    public static Color Fond => Couleur("Fond");
    public static Color Bordure => Couleur("Bordure");
    public static Color TexteSecondaire => Couleur("TexteSecondaire");
    public static Color TexteDesactive => Couleur("TexteDesactive");
    public static Color Erreur => Couleur("Erreur");
    public static Color Succes => Couleur("Succes");
    public static Color SuccesClair => Couleur("SuccesClair");
    public static Color Attention => Couleur("Attention");
    public static Color AttentionClair => Couleur("AttentionClair");
    public static Color Desactive => Couleur("Desactive");
    public static Color Surface => Couleur("Surface");

    public const string PoliceNormale = "HankenRegular";
    public const string PoliceDemiGras = "HankenSemiBold";
    public const string PoliceGras = "HankenBold";
}
