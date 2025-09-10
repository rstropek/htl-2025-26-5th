using Avalonia.Controls;
using AvaloniaBasics.ViewModels;

namespace AvaloniaBasics.Views;

public partial class TodoListView : UserControl
{
    public TodoListView()
    {
        InitializeComponent();
    }

    public TodoListView(TodoListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
