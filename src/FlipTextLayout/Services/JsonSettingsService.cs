using System.IO;
using System.Text.Json;
using FlipTextLayout.Models;
using Microsoft.Extensions.Logging;

namespace FlipTextLayout.Services;

public sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly ILogger<JsonSettingsService> _logger;
    private readonly string _settingsPath;

    public JsonSettingsService(ILogger<JsonSettingsService> logger)
    {
        _logger = logger;
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsPath = Path.Combine(appData, "FlipTextLayout", "settings.json");
    }

    public event EventHandler<AppSettings>? SettingsChanged;

    public AppSettings Current { get; private set; } = new();

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                Current = new AppSettings();
                await SaveFileAsync(Current, cancellationToken);
                return Current;
            }

            await using FileStream stream = File.OpenRead(_settingsPath);
            AppSettings? settings = await JsonSerializer.DeserializeAsync<AppSettings>(
                stream,
                JsonOptions,
                cancellationToken);

            Current = settings ?? new AppSettings();
            Normalize(Current);
            return Current;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load settings. Defaults will be used.");
            Current = new AppSettings();
            return Current;
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        Normalize(settings);
        await SaveFileAsync(settings, cancellationToken);
        Current = settings;
        SettingsChanged?.Invoke(this, Current);
    }

    private async Task SaveFileAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);

        await using FileStream stream = File.Create(_settingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
    }

    private static void Normalize(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Hotkey.Key))
        {
            settings.Hotkey.Key = HotkeyGesture.Default.Key;
        }
    }
}
