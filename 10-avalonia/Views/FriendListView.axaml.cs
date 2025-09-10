using Avalonia.Controls;
using AvaloniaBasics.ViewModels;

namespace AvaloniaBasics.Views;

public partial class FriendListView : UserControl
{
    public FriendListView()
    {
        InitializeComponent();
    }

    public FriendListView(FriendListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
