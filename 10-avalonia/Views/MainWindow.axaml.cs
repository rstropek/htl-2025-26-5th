using Avalonia.Controls;
using AvaloniaBasics.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaBasics.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider serviceProvider;
    private CalculatorViewModel? calculatorViewModel;
    private TodoListViewModel? todoListViewModel;
    private FriendListViewModel? friendListViewModel;

    public MainWindow(IServiceProvider serviceProvider)
    {
        // Store the service provider for later use. We will use it
        // to resolve the ViewModels when they are needed.
        this.serviceProvider = serviceProvider;

        // The call to InitializeComponent is required to initialize the window.
        InitializeComponent();

        // The main window is very simple. It has no noteworthy logic.
        // Therefore, we do not need a view model. Instead, we set the DataContext
        // to itself, so that we can bind to its properties (like CalculatorViewModel).
        DataContext = this;
    }

    // Here we create the CalculatorViewModel using DI. The ViewLocator will
    // automatically resolve this to CalculatorView and set the DataContext.
    public CalculatorViewModel CalculatorViewModel => calculatorViewModel ??= serviceProvider.GetRequiredService<CalculatorViewModel>();

    // Here we create the TodoListViewModel using DI. The ViewLocator will
    // automatically resolve this to TodoListView and set the DataContext.
    public TodoListViewModel TodoListViewModel => todoListViewModel ??= serviceProvider.GetRequiredService<TodoListViewModel>();

    // Here we create the FriendListViewModel using DI. The ViewLocator will
    // automatically resolve this to FriendListView and set the DataContext.
    public FriendListViewModel FriendListViewModel => friendListViewModel ??= serviceProvider.GetRequiredService<FriendListViewModel>();
}