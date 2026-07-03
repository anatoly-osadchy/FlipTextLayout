using FlipTextLayout.Models;

namespace FlipTextLayout.Services;

public interface IClipboardService : IDisposable
{
    uint GetSequenceNumber();

    bool ContainsText();

    string? GetText();

    void SetText(string text);

    Task<bool> WaitForClipboardChangeAsync(
        uint previousSequenceNumber,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}

public interface IHotkeyService : IDisposable
{
    event EventHandler? HotkeyPressed;

    void Register(HotkeyGesture hotkey);

    void Unregister();
}

public interface IKeyboardService
{
    void SendCopy();

    Task SendPasteAsync(CancellationToken cancellationToken);

    void SwitchInputLanguage(TextLayout targetLayout);
}

public interface ILayoutConverter
{
    LayoutConversionResult Convert(string text);
}

public interface ISettingsService
{
    event EventHandler<AppSettings>? SettingsChanged;

    AppSettings Current { get; }

    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}

public interface ITrayIconService : IDisposable
{
    void Initialize(
        Func<Task> switchLayoutAsync,
        Action showSettings,
        Action exit);
}

public interface IWindowsStartupService
{
    void Apply(bool enabled);
}
