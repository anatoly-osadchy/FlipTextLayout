using System.Runtime.InteropServices;
using System.Windows.Interop;
using WpfClipboard = System.Windows.Clipboard;
using WpfTextDataFormat = System.Windows.TextDataFormat;

namespace FlipTextLayout.Services;

public sealed class WpfClipboardService : IClipboardService
{
    private const int WmClipboardUpdate = 0x031D;
    private readonly HwndSource _source;
    private readonly List<ClipboardWaiter> _waiters = [];
    private bool _disposed;

    public WpfClipboardService()
    {
        HwndSourceParameters parameters = new("FlipTextLayoutClipboardWindow")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0
        };

        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
        AddClipboardFormatListener(_source.Handle);
    }

    public uint GetSequenceNumber()
    {
        return GetClipboardSequenceNumber();
    }

    public bool ContainsText()
    {
        return WpfClipboard.ContainsText(WpfTextDataFormat.UnicodeText);
    }

    public string? GetText()
    {
        return ContainsText()
            ? WpfClipboard.GetText(WpfTextDataFormat.UnicodeText)
            : null;
    }

    public void SetText(string text)
    {
        WpfClipboard.SetText(text, WpfTextDataFormat.UnicodeText);
    }

    public Task<bool> WaitForClipboardChangeAsync(
        uint previousSequenceNumber,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (GetSequenceNumber() != previousSequenceNumber)
        {
            return Task.FromResult(true);
        }

        ClipboardWaiter waiter = new(previousSequenceNumber);

        lock (_waiters)
        {
            _waiters.Add(waiter);
        }

        _ = CompleteAfterTimeoutAsync(waiter, timeout, cancellationToken);
        return waiter.Task;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        RemoveClipboardFormatListener(_source.Handle);
        _source.RemoveHook(WndProc);
        _source.Dispose();
        _disposed = true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmClipboardUpdate)
        {
            CompleteChangedWaiters();
        }

        return IntPtr.Zero;
    }

    private void CompleteChangedWaiters()
    {
        uint currentSequenceNumber = GetSequenceNumber();
        ClipboardWaiter[] completed;

        lock (_waiters)
        {
            completed = _waiters
                .Where(waiter => currentSequenceNumber != waiter.PreviousSequenceNumber)
                .ToArray();

            foreach (ClipboardWaiter waiter in completed)
            {
                _waiters.Remove(waiter);
            }
        }

        foreach (ClipboardWaiter waiter in completed)
        {
            waiter.TrySetResult(true);
        }
    }

    private async Task CompleteAfterTimeoutAsync(
        ClipboardWaiter waiter,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(timeout, cancellationToken);
            RemoveWaiter(waiter);
            waiter.TrySetResult(false);
        }
        catch (OperationCanceledException)
        {
            RemoveWaiter(waiter);
            waiter.TrySetCanceled(cancellationToken);
        }
    }

    private void RemoveWaiter(ClipboardWaiter waiter)
    {
        lock (_waiters)
        {
            _waiters.Remove(waiter);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    private sealed class ClipboardWaiter
    {
        private readonly TaskCompletionSource<bool> _source = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ClipboardWaiter(uint previousSequenceNumber)
        {
            PreviousSequenceNumber = previousSequenceNumber;
        }

        public uint PreviousSequenceNumber { get; }

        public Task<bool> Task => _source.Task;

        public void TrySetResult(bool result)
        {
            _source.TrySetResult(result);
        }

        public void TrySetCanceled(CancellationToken cancellationToken)
        {
            _source.TrySetCanceled(cancellationToken);
        }
    }
}
