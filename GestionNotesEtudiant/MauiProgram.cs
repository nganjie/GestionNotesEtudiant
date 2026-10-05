using GestionNotesEtudiant.Services;
using GestionNotesEtudiant.ViewModels;
using GestionNotesEtudiant.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace GestionNotesEtudiant;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("HankenGrotesk-Regular.ttf", "HankenRegular");
                fonts.AddFont("HankenGrotesk-SemiBold.ttf", "HankenSemiBold");
                fonts.AddFont("HankenGrotesk-Bold.ttf", "HankenBold");
            });

        SupprimerSoulignementAndroid();

        builder.Services.AddSingleton<DonneesService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ExportService>();

        Ecran<ConnexionPage, ConnexionViewModel>(builder.Services);
        Ecran<InscriptionPage, InscriptionViewModel>(builder.Services);
        Ecran<MotDePasseOubliePage, MotDePasseOublieViewModel>(builder.Services);
        Ecran<AccueilPage, AccueilViewModel>(builder.Services);
        Ecran<UePage, UeViewModel>(builder.Services);
        Ecran<NouvelleEvaluationPage, NouvelleEvaluationViewModel>(builder.Services);
        Ecran<SaisiePage, SaisieViewModel>(builder.Services);
        Ecran<MoyennesPage, MoyennesViewModel>(builder.Services);
        Ecran<PublicationPage, PublicationViewModel>(builder.Services);
        Ecran<ExportPage, ExportViewModel>(builder.Services);
        Ecran<EtudiantsPage, EtudiantsViewModel>(builder.Services);
        Ecran<FicheEtudiantPage, FicheEtudiantViewModel>(builder.Services);
        Ecran<ProfilPage, ProfilViewModel>(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static void Ecran<TPage, TVueModele>(IServiceCollection services)
        where TPage : class
        where TVueModele : class
    {
        services.AddTransient<TPage>();
        services.AddTransient<TVueModele>();
    }

    /// <summary>Les champs sont dessinés par ChampSaisie : on retire le trait natif d'Android.</summary>
    static void SupprimerSoulignementAndroid()
    {
        EntryHandler.Mapper.AppendToMapping("SansSoulignement", (handler, _) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });
        DatePickerHandler.Mapper.AppendToMapping("SansSoulignement", (handler, _) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
        });
    }
}
