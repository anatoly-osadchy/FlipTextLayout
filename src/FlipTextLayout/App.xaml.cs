using System.Windows;
using FlipTextLayout.Services;
using FlipTextLayout.ViewModels;
using FlipTextLayout.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlipTextLayout;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _services = ConfigureServices();
        await _services.GetRequiredService<ApplicationController>().StartAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }

    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<ApplicationController>();
        services.AddSingleton<ISettingsService, JsonSettingsService>();
        services.AddSingleton<IWindowsStartupService, WindowsStartupService>();
        services.AddSingleton<IClipboardService, WpfClipboardService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IKeyboardService, KeyboardService>();
        services.AddSingleton<ILayoutConverter, RussianEnglishLayoutConverter>();
        services.AddSingleton<ITrayIconService, TrayIconService>();

        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsWindow>();

        return services.BuildServiceProvider();
    }
}
