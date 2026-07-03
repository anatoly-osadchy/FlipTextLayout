using System.Media;
using System.ComponentModel;
using System.Windows;
using FlipTextLayout.Models;
using FlipTextLayout.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlipTextLayout.Services;

public sealed class ApplicationController : IDisposable
{
    private static readonly TimeSpan ClipboardTimeout = TimeSpan.FromMilliseconds(1200);
    private readonly IServiceProvider _services;
    private readonly ISettingsService _settingsService;
    private readonly IWindowsStartupService _startupService;
    private readonly IClipboardService _clipboardService;
    private readonly IHotkeyService _hotkeyService;
    private readonly IKeyboardService _keyboardService;
    private readonly ILayoutConverter _layoutConverter;
    private readonly ITrayIconService _trayIconService;
    private readonly ILogger<ApplicationController> _logger;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private bool _disposed;

    public ApplicationController(
        IServiceProvider services,
        ISettingsService settingsService,
        IWindowsStartupService startupService,
        IClipboardService clipboardService,
        IHotkeyService hotkeyService,
        IKeyboardService keyboardService,
        ILayoutConverter layoutConverter,
        ITrayIconService trayIconService,
        ILogger<ApplicationController> logger)
    {
        _services = services;
        _settingsService = settingsService;
        _startupService = startupService;
        _clipboardService = clipboardService;
        _hotkeyService = hotkeyService;
        _keyboardService = keyboardService;
        _layoutConverter = layoutConverter;
        _trayIconService = trayIconService;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        AppSettings settings = await _settingsService.LoadAsync(CancellationToken.None);
        ApplySettings(settings);

        _settingsService.SettingsChanged += OnSettingsChanged;
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

        _trayIconService.Initialize(
            SwitchLayoutAsync,
            ShowSettings,
            Exit);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _settingsService.SettingsChanged -= OnSettingsChanged;
        _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
        _operationLock.Dispose();
        _disposed = true;
    }

    private void ApplySettings(AppSettings settings)
    {
        try
        {
            _hotkeyService.Register(settings.Hotkey);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1409 && !settings.Hotkey.EqualsGesture(HotkeyGesture.Default))
        {
            _logger.LogWarning(
                ex,
                "Hotkey {Hotkey} is already registered. Falling back to {FallbackHotkey}.",
                settings.Hotkey,
                HotkeyGesture.Default);

            settings.Hotkey = HotkeyGesture.Default;

            try
            {
                _hotkeyService.Register(settings.Hotkey);
                _ = _settingsService.SaveAsync(settings, CancellationToken.None);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(
                    fallbackEx,
                    "Failed to register fallback hotkey {Hotkey}.",
                    settings.Hotkey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register hotkey {Hotkey}.", settings.Hotkey);
        }

        _startupService.Apply(settings.StartWithWindows);
    }

    private void OnSettingsChanged(object? sender, AppSettings settings)
    {
        ApplySettings(settings);
    }

    private async void OnHotkeyPressed(object? sender, EventArgs e)
    {
        await SwitchLayoutAsync();
    }

    private async Task SwitchLayoutAsync()
    {
        if (!await _operationLock.WaitAsync(0))
        {
            return;
        }

        string? previousText = null;
        bool shouldRestore = false;

        try
        {
            AppSettings settings = _settingsService.Current;
            shouldRestore = settings.RestoreClipboard;

            if (_clipboardService.ContainsText())
            {
                previousText = _clipboardService.GetText();
            }

            uint sequenceNumber = _clipboardService.GetSequenceNumber();
            _keyboardService.SendCopy();

            bool changed = await _clipboardService.WaitForClipboardChangeAsync(
                sequenceNumber,
                ClipboardTimeout,
                CancellationToken.None);

            if (!changed)
            {
                return;
            }

            if (!_clipboardService.ContainsText())
            {
                return;
            }

            string? selectedText = _clipboardService.GetText();

            if (string.IsNullOrEmpty(selectedText))
            {
                return;
            }

            LayoutConversionResult result = _layoutConverter.Convert(selectedText);

            if (!result.Changed)
            {
                return;
            }

            _clipboardService.SetText(result.Text);
            await _keyboardService.SendPasteAsync(CancellationToken.None);

            if (settings.SwitchWindowsKeyboardLayout)
            {
                _keyboardService.SwitchInputLanguage(result.TargetLayout);
            }

            if (settings.PlaySoundAfterConversion)
            {
                SystemSounds.Asterisk.Play();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Layout switching failed.");
        }
        finally
        {
            if (shouldRestore && previousText is not null)
            {
                TryRestoreClipboard(previousText);
            }

            _operationLock.Release();
        }
    }

    private void TryRestoreClipboard(string text)
    {
        try
        {
            _clipboardService.SetText(text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to restore previous clipboard text.");
        }
    }

    private void ShowSettings()
    {
        SettingsWindow window = _services.GetRequiredService<SettingsWindow>();
        window.Show();
        window.Activate();
    }

    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
