using Microsoft.Win32;

namespace FlipTextLayout.Services;

public sealed class WindowsStartupService : IWindowsStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "FlipTextLayout";

    public void Apply(bool enabled)
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);

        if (key is null)
        {
            return;
        }

        if (enabled)
        {
            string? processPath = Environment.ProcessPath;

            if (!string.IsNullOrWhiteSpace(processPath))
            {
                key.SetValue(ValueName, $"\"{processPath}\"");
            }
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
