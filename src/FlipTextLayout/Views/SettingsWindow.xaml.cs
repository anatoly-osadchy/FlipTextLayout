using System.Windows;
using FlipTextLayout.ViewModels;

namespace FlipTextLayout.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is SettingsViewModel viewModel)
        {
            viewModel.RequestClose -= OnRequestClose;
        }

        base.OnClosed(e);
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        Close();
    }
}
