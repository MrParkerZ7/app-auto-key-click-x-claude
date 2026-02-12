using System.Windows;
using AutoClickKey.ViewModels;

namespace AutoClickKey;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel = DataContext as MainViewModel;
        _viewModel?.InitializeHotkeys(this);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _viewModel?.Dispose();
    }
}
