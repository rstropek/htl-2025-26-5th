using System.Collections.ObjectModel;
using AvaloniaBasics.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaBasics.ViewModels;

public partial class TodoListViewModel : ViewModelBase
{
    // Property for the new todo item text that the user will enter
    // Note that we need to notify a related command when this changes
    // as its ability to execute depends on this property.
    // TODO: Read https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty#notifying-dependent-commands
    //       to get a deeper understanding of `NotifyCanExecuteChangedFor`
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(AddTodoCommand))]
    private string newTodoText = string.Empty;

    // ObservableCollection to hold all todo items. Note that here
    // it is particularly important to use an ObservableCollection
    // so that the UI can automatically update when items are added or removed.
    public ObservableCollection<TodoItem> TodoItems { get; } = [];

    // Command to add a new todo item
    [RelayCommand(CanExecute = nameof(CanAddTodo))]
    private void AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTodoText))
        {
            TodoItems.Add(new TodoItem(NewTodoText.Trim()));
            NewTodoText = string.Empty;
        }
    }

    // Check if we can add a todo (text is not empty)
    private bool CanAddTodo() => !string.IsNullOrWhiteSpace(NewTodoText);

    // Command to delete a todo item
    [RelayCommand]
    private void DeleteTodo(TodoItem? todoItem)
    {
        if (todoItem != null)
        {
            TodoItems.Remove(todoItem);
        }
    }

    // Command to clear all todo items
    [RelayCommand]
    private void ClearAll()
    {
        TodoItems.Clear();
        NewTodoText = string.Empty;
    }

    // Command to delete completed items
    [RelayCommand]
    private void ClearCompleted()
    {
        for (int i = TodoItems.Count - 1; i >= 0; i--)
        {
            if (TodoItems[i].IsDone)
            {
                TodoItems.RemoveAt(i);
            }
        }
    }
}
