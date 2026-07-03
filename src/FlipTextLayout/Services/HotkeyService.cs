using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using FlipTextLayout.Models;

namespace FlipTextLayout.Services;

public sealed class HotkeyService : IHotkeyService
{
    private const int HotkeyId = 0x464C;
    private const int WmHotkey = 0x0312;
    private readonly HwndSource _source;
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
        Unregister();

        uint modifiers = BuildModifiers(hotkey);
        int virtualKey = KeyInterop.VirtualKeyFromKey(hotkey.ParsedKey);

        if (virtualKey == 0)
        {
            throw new InvalidOperationException($"Unsupported hotkey key: {hotkey.Key}");
        }

        if (!RegisterHotKey(_source.Handle, HotkeyId, modifiers, (uint)virtualKey))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Failed to register hotkey {hotkey}.");
        }

        _registered = true;
    }

    public void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(_source.Handle, HotkeyId);
        _registered = false;
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
