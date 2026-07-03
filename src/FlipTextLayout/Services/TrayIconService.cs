using System.Windows.Forms;
using DrawingIcon = System.Drawing.Icon;
using DrawingSystemIcons = System.Drawing.SystemIcons;

namespace FlipTextLayout.Services;

public sealed class TrayIconService : ITrayIconService
{
    private NotifyIcon? _notifyIcon;
    private bool _disposed;

    public void Initialize(
        Func<Task> switchLayoutAsync,
        Action showSettings,
        Action exit)
    {
        ContextMenuStrip menu = new();

        ToolStripMenuItem switchItem = new("Switch Layout");
        switchItem.Click += async (_, _) => await switchLayoutAsync();

        ToolStripMenuItem settingsItem = new("Settings");
        settingsItem.Click += (_, _) => showSettings();

        ToolStripMenuItem exitItem = new("Exit");
        exitItem.Click += (_, _) => exit();

        menu.Items.Add(switchItem);
        menu.Items.Add(settingsItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        DrawingIcon icon = DrawingSystemIcons.Application;

        _notifyIcon = new NotifyIcon
        {
            Icon = icon,
            Text = "FlipTextLayout",
            ContextMenuStrip = menu,
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => showSettings();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        _disposed = true;
    }
}
