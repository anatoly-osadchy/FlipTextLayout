using System.Runtime.InteropServices;
using FlipTextLayout.Models;

namespace FlipTextLayout.Services;

public sealed class KeyboardService : IKeyboardService
{
    private const ushort VkControl = 0x11;
    private const ushort VkC = 0x43;
    private const ushort VkV = 0x56;
    private const uint KeyEventKeyUp = 0x0002;
    private const uint WmInputLangChangeRequest = 0x0050;
    private const string EnglishLayoutId = "00000409";
    private const string RussianLayoutId = "00000419";

    public void SendCopy()
    {
        SendChord(VkControl, VkC);
    }

    public async Task SendPasteAsync(CancellationToken cancellationToken)
    {
        SendChord(VkControl, VkV);
        await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
    }

    public void SwitchInputLanguage(TextLayout targetLayout)
    {
        string? layoutId = targetLayout switch
        {
            TextLayout.English => EnglishLayoutId,
            TextLayout.Russian => RussianLayoutId,
            _ => null
        };

        if (layoutId is null)
        {
            return;
        }

        IntPtr foregroundWindow = GetForegroundWindow();

        if (foregroundWindow == IntPtr.Zero)
        {
            return;
        }

        IntPtr keyboardLayout = LoadKeyboardLayout(layoutId, 1);

        if (keyboardLayout == IntPtr.Zero)
        {
            return;
        }

        PostMessage(foregroundWindow, WmInputLangChangeRequest, IntPtr.Zero, keyboardLayout);
    }

    private static void SendChord(ushort modifierKey, ushort key)
    {
        Input[] inputs =
        [
            Input.KeyDown(modifierKey),
            Input.KeyDown(key),
            Input.KeyUp(key),
            Input.KeyUp(modifierKey)
        ];

        uint sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());

        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("SendInput failed.");
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint numberOfInputs, Input[] inputs, int size);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint flags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;

        public KeyboardInput KeyboardInput;

        public static Input KeyDown(ushort key)
        {
            return new Input
            {
                Type = 1,
                KeyboardInput = new KeyboardInput
                {
                    VirtualKey = key
                }
            };
        }

        public static Input KeyUp(ushort key)
        {
            return new Input
            {
                Type = 1,
                KeyboardInput = new KeyboardInput
                {
                    VirtualKey = key,
                    Flags = KeyEventKeyUp
                }
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;

        public ushort Scan;

        public uint Flags;

        public uint Time;

        public IntPtr ExtraInfo;
    }
}
