using System.Windows.Input;

namespace FlipTextLayout.Models;

public sealed class AppSettings
{
    public HotkeyGesture Hotkey { get; set; } = HotkeyGesture.Default;

    public bool RestoreClipboard { get; set; } = true;

    public bool SwitchWindowsKeyboardLayout { get; set; }

    public bool StartWithWindows { get; set; }

    public bool PlaySoundAfterConversion { get; set; } = true;
}

public sealed class HotkeyGesture
{
    public static HotkeyGesture Default => new()
    {
        Control = true,
        Shift = true,
        Key = nameof(System.Windows.Input.Key.Space)
    };

    public bool Control { get; set; }

    public bool Alt { get; set; }

    public bool Shift { get; set; }

    public bool Windows { get; set; }

    public string Key { get; set; } = nameof(System.Windows.Input.Key.Space);

    public Key ParsedKey
    {
        get
        {
            if (Enum.TryParse(Key, ignoreCase: true, out Key parsedKey))
            {
                return parsedKey;
            }

            return System.Windows.Input.Key.Space;
        }
    }

    public override string ToString()
    {
        List<string> parts = [];

        if (Control)
        {
            parts.Add("Ctrl");
        }

        if (Alt)
        {
            parts.Add("Alt");
        }

        if (Shift)
        {
            parts.Add("Shift");
        }

        if (Windows)
        {
            parts.Add("Win");
        }

        parts.Add(ParsedKey.ToString());
        return string.Join(" + ", parts);
    }
}

public enum TextLayout
{
    Unknown,
    English,
    Russian
}

public sealed record LayoutConversionResult(
    string Text,
    TextLayout SourceLayout,
    TextLayout TargetLayout,
    bool Changed);
