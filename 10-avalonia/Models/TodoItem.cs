using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaBasics.Models;

public partial class TodoItem : ObservableObject
{
    [ObservableProperty] private bool isDone;
    [ObservableProperty] private string text = string.Empty;

    public TodoItem(string text)
    {
        Text = text;
        IsDone = false;
    }
}
