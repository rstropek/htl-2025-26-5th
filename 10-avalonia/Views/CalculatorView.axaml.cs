using Avalonia.Controls;
using AvaloniaBasics.ViewModels;

namespace AvaloniaBasics.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
    }

    public CalculatorView(CalculatorViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
