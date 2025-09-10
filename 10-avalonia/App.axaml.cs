using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaBasics.ViewModels;
using AvaloniaBasics.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaBasics;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // This is how we setup dependency injection in Avalonia. We use the built-in,
            // lightweight DI container from Microsoft.Extensions.DependencyInjection.
            // With the ViewLocator pattern, we only need to register ViewModels and the MainWindow.
            // The ViewLocator will automatically create and wire up the corresponding Controls.
            var collection = new ServiceCollection();
            collection.AddTransient<MainWindow>();

            // Note that we do not need to publish the controls. They are automatically
            // found based on their names by ViewLocator.cs
            collection.AddTransient<CalculatorViewModel>();
            collection.AddTransient<TodoListViewModel>();
            collection.AddTransient<FriendListViewModel>();
            var services = collection.BuildServiceProvider();

            // It is important to create the main window via the service provider,
            // so that all dependencies are injected properly.
            desktop.MainWindow = services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}