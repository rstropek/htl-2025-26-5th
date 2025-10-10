using Avalonia.Controls;
using TvGuide.UI.ViewModels;

namespace TvGuide.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}