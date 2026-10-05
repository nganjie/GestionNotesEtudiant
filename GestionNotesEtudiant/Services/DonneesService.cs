using System.Text.Json;
using GestionNotesEtudiant.Models;

namespace GestionNotesEtudiant.Services;

/// <summary>
/// Stockage local des données dans un unique fichier JSON (pas de serveur,
/// pas d'appel réseau inutile). Les données sont chargées une seule fois en mémoire.
/// </summary>
public class DonneesService
{
    static readonly JsonSerializerOptions OptionsJson = new() { WriteIndented = false };

    readonly SemaphoreSlim _verrou = new(1, 1);
    readonly string _chemin = Path.Combine(FileSystem.AppDataDirectory, "bareme.json");
    BaseDonnees? _base;

    public BaseDonnees Base => _base ?? throw new InvalidOperationException("Données non chargées.");

    public bool EstCharge => _base is not null;

    public async Task ChargerAsync()
    {
        if (_base is not null)
            return;

        await _verrou.WaitAsync();
        try
        {
            if (_base is not null)
                return;

            if (File.Exists(_chemin))
            {
                await using var flux = File.OpenRead(_chemin);
                _base = await JsonSerializer.DeserializeAsync<BaseDonnees>(flux, OptionsJson);
            }

            if (_base is null)
            {
                _base = JeuDeDonnees.Creer();
                await EcrireAsync();
            }
        }
        finally
        {
            _verrou.Release();
        }
    }

    public async Task EnregistrerAsync()
    {
        await _verrou.WaitAsync();
        try
        {
            await EcrireAsync();
        }
        finally
        {
            _verrou.Release();
        }
    }

    async Task EcrireAsync()
    {
        var temporaire = _chemin + ".tmp";
        await using (var flux = File.Create(temporaire))
        {
            await JsonSerializer.SerializeAsync(flux, _base, OptionsJson);
        }
        File.Move(temporaire, _chemin, overwrite: true);
    }

    public UniteEnseignement? Unite(string code) =>
        Base.Unites.FirstOrDefault(u => u.Code == code);

    public IReadOnlyList<Etudiant> EtudiantsDe(string promotion) =>
        Base.Etudiants
            .Where(e => e.Promotion == promotion)
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .ToList();

    public Etudiant? Etudiant(string matricule) =>
        Base.Etudiants.FirstOrDefault(e => e.Matricule == matricule);

    public IReadOnlyList<string> Promotions =>
        Base.Etudiants.Select(e => e.Promotion).Distinct().OrderByDescending(p => p).ToList();
}
