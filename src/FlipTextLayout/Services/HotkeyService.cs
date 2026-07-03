using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using FlipTextLayout.Models;

namespace FlipTextLayout.Services;

public sealed class HotkeyService : IHotkeyService
{
    private const int HotkeyId = 0x464C;
    private const int TestHotkeyId = 0x464D;
    private const int WmHotkey = 0x0312;
    private readonly HwndSource _source;
    private HotkeyGesture? _registeredHotkey;
    private bool _registered;
    private bool _disposed;

    public HotkeyService()
    {
        HwndSourceParameters parameters = new("FlipTextLayoutHotkeyWindow")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    public event EventHandler? HotkeyPressed;

    public void Register(HotkeyGesture hotkey)
    {
        if (_registeredHotkey?.EqualsGesture(hotkey) == true)
        {
            return;
        }

        HotkeyNativeValues values = GetNativeValues(hotkey);

        if (!TryRegisterTestHotkey(values, out int errorCode))
        {
            throw new Win32Exception(errorCode, $"Failed to register hotkey {hotkey}.");
        }

        Unregister();

        if (!RegisterHotKey(_source.Handle, HotkeyId, values.Modifiers, values.VirtualKey))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to register hotkey {hotkey}.");
        }

        _registeredHotkey = Clone(hotkey);
        _registered = true;
    }

    public bool IsHotkeyAvailable(HotkeyGesture hotkey)
    {
        if (_registeredHotkey?.EqualsGesture(hotkey) == true)
        {
            return true;
        }

        HotkeyNativeValues values;

        try
        {
            values = GetNativeValues(hotkey);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        return TryRegisterTestHotkey(values, out _);
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(_source.Handle, HotkeyId);
        _registered = false;
        _registeredHotkey = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Unregister();
        _source.RemoveHook(WndProc);
        _source.Dispose();
        _disposed = true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    private static uint BuildModifiers(HotkeyGesture hotkey)
    {
        uint modifiers = 0;

        if (hotkey.Control)
        {
            modifiers |= 0x0002;
        }

        if (hotkey.Alt)
        {
            modifiers |= 0x0001;
        }

        if (hotkey.Shift)
        {
            modifiers |= 0x0004;
        }

        if (hotkey.Windows)
        {
            modifiers |= 0x0008;
        }

        return modifiers;
    }

    private HotkeyNativeValues GetNativeValues(HotkeyGesture hotkey)
    {
        uint modifiers = BuildModifiers(hotkey);

        if (!Enum.TryParse(hotkey.Key, ignoreCase: true, out Key parsedKey))
        {
            throw new InvalidOperationException($"Unsupported hotkey key: {hotkey.Key}");
        }

        int virtualKey = KeyInterop.VirtualKeyFromKey(parsedKey);

        if (virtualKey == 0)
        {
            throw new InvalidOperationException($"Unsupported hotkey key: {hotkey.Key}");
        }

        return new HotkeyNativeValues(modifiers, (uint)virtualKey);
    }

    private bool TryRegisterTestHotkey(HotkeyNativeValues values, out int errorCode)
    {
        if (!RegisterHotKey(_source.Handle, TestHotkeyId, values.Modifiers, values.VirtualKey))
        {
            errorCode = Marshal.GetLastWin32Error();
            return false;
        }

        UnregisterHotKey(_source.Handle, TestHotkeyId);
        errorCode = 0;
        return true;
    }

    private static HotkeyGesture Clone(HotkeyGesture hotkey)
    {
        return new HotkeyGesture
        {
            Control = hotkey.Control,
            Alt = hotkey.Alt,
            Shift = hotkey.Shift,
            Windows = hotkey.Windows,
            Key = hotkey.Key
        };
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private sealed record HotkeyNativeValues(uint Modifiers, uint VirtualKey);
}
