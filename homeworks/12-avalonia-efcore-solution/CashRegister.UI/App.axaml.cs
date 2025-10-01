using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using CashRegister.UI.ViewModels;
using CashRegister.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CashRegister.Data;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.UI;

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

            // Add support for appsettings.json
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = configurationBuilder.Build();

            // This is how we setup dependency injection in Avalonia. We use the built-in,
            // lightweight DI container from Microsoft.Extensions.DependencyInjection.
            var collection = new ServiceCollection();
            collection.AddTransient<MainWindow>();
            collection.AddTransient<MainWindowViewModel>();
            collection.AddDbContextFactory<ApplicationDataContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("CashRegister")));
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