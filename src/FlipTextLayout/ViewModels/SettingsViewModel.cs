using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FlipTextLayout.Models;
using FlipTextLayout.Services;

namespace FlipTextLayout.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private bool _control;
    private bool _alt;
    private bool _shift;
    private bool _windows;
    private string _key = nameof(System.Windows.Input.Key.Space);
    private bool _restoreClipboard;
    private bool _switchWindowsKeyboardLayout;
    private bool _startWithWindows;
    private bool _playSoundAfterConversion;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromSettings(settingsService.Current);
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? RequestClose;

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public bool Control
    {
        get => _control;
        set => SetField(ref _control, value);
    }

    public bool Alt
    {
        get => _alt;
        set => SetField(ref _alt, value);
    }

    public bool Shift
    {
        get => _shift;
        set => SetField(ref _shift, value);
    }

    public bool Windows
    {
        get => _windows;
        set => SetField(ref _windows, value);
    }

    public string Key
    {
        get => _key;
        set => SetField(ref _key, value);
    }

    public bool RestoreClipboard
    {
        get => _restoreClipboard;
        set => SetField(ref _restoreClipboard, value);
    }

    public bool SwitchWindowsKeyboardLayout
    {
        get => _switchWindowsKeyboardLayout;
        set => SetField(ref _switchWindowsKeyboardLayout, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetField(ref _startWithWindows, value);
    }

    public bool PlaySoundAfterConversion
    {
        get => _playSoundAfterConversion;
        set => SetField(ref _playSoundAfterConversion, value);
    }

    private async Task SaveAsync()
    {
        AppSettings settings = new()
        {
            Hotkey = new HotkeyGesture
            {
                Control = Control,
                Alt = Alt,
                Shift = Shift,
                Windows = Windows,
                Key = Key
            },
            RestoreClipboard = RestoreClipboard,
            SwitchWindowsKeyboardLayout = SwitchWindowsKeyboardLayout,
            StartWithWindows = StartWithWindows,
            PlaySoundAfterConversion = PlaySoundAfterConversion
        };

        await _settingsService.SaveAsync(settings, CancellationToken.None);
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void LoadFromSettings(AppSettings settings)
    {
        Control = settings.Hotkey.Control;
        Alt = settings.Hotkey.Alt;
        Shift = settings.Hotkey.Shift;
        Windows = settings.Hotkey.Windows;
        Key = settings.Hotkey.Key;
        RestoreClipboard = settings.RestoreClipboard;
        SwitchWindowsKeyboardLayout = settings.SwitchWindowsKeyboardLayout;
        StartWithWindows = settings.StartWithWindows;
        PlaySoundAfterConversion = settings.PlaySoundAfterConversion;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
