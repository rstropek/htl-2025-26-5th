using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;

namespace MyApp;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this; // Essential for binding - makes this window the binding source
    }

    /// <summary>
    /// Demonstrates basic command binding without parameters.
    /// The [RelayCommand] attribute automatically generates a 'NormalClickCommand' property
    /// that can be bound to in XAML using {Binding NormalClickCommand}.
    /// </summary>
    [RelayCommand]
    private async Task NormalClick()
    {
        await MessageBoxManager
            .GetMessageBoxStandard("Info", "Normal Click executed")
            .ShowWindowAsync();
    }

    /// <summary>
    /// Demonstrates command binding with a string parameter.
    /// The CommandParameter in XAML will be passed to this method.
    /// Shows how literal values can be passed as parameters.
    /// </summary>
    /// <param name="parameterAsString">The string parameter from CommandParameter binding</param>
    [RelayCommand]
    private async Task ClickWithParameter(string parameterAsString)
    {
        await MessageBoxManager
            .GetMessageBoxStandard("Info", $"Click with Parameter executed with value {parameterAsString}")
            .ShowWindowAsync();
    }

    /// <summary>
    /// Demonstrates self-referencing binding using $self keyword.
    /// The CommandParameter="{Binding $self.Content}" passes the button's own Content property.
    /// This is useful for accessing properties of the UI element that triggered the command.
    /// </summary>
    /// <param name="buttonContent">The Content property of the button that was clicked</param>
    [RelayCommand]
    private async Task ClickWithContent(object buttonContent)
    {
        string contentText = buttonContent?.ToString() ?? "No content";
        await MessageBoxManager
            .GetMessageBoxStandard("Info", $"Button content: '{contentText}'")
            .ShowWindowAsync();
    }

    /// <summary>
    /// Demonstrates command binding with type conversion using a value converter.
    /// The CommandParameter uses a StringToIntConverter to convert the button's Content (string)
    /// to an integer parameter. This shows how converters enable type-safe parameter passing.
    /// </summary>
    /// <param name="parameter">The integer parameter converted from the button's content</param>
    [RelayCommand]
    private async Task ClickWithIntParameter(int parameter)
    {
        await MessageBoxManager
            .GetMessageBoxStandard("Info", $"Click with int Parameter executed with value {parameter}")
            .ShowWindowAsync();
    }
}

/// <summary>
/// Value converter that transforms string values to integers for data binding.
/// 
/// Value converters are essential in Avalonia binding when you need to:
/// - Convert between different data types
/// - Transform data for display purposes
/// - Enable binding between incompatible property types
/// 
/// This converter is used in XAML with the Converter={StaticResource StringToIntConverter} syntax.
/// It's registered as a static resource in the Window.Resources section.
/// 
/// Binding flow: Button.Content (string) → Converter → Command parameter (int)
/// </summary>
public class StringToIntConverter : IValueConverter
{
    /// <summary>
    /// Converts a string value to an integer.
    /// This method is called when data flows from the binding source to the target.
    /// </summary>
    /// <param name="value">The source value (expected to be a string)</param>
    /// <param name="targetType">The type of the binding target property (int in this case)</param>
    /// <param name="parameter">Optional converter parameter (not used here)</param>
    /// <param name="culture">Culture information for localization (not used here)</param>
    /// <returns>Integer value if conversion succeeds, 0 if it fails</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && int.TryParse(str, out int result))
        {
            return result;
        }

        return 0; // default value if conversion fails
    }

    /// <summary>
    /// Converts back from integer to string (not implemented as it's not needed for command parameters).
    /// This would be used for two-way binding scenarios.
    /// </summary>
    /// <param name="value">The target value</param>
    /// <param name="targetType">The type of the binding source property</param>
    /// <param name="parameter">Optional converter parameter</param>
    /// <param name="culture">Culture information for localization</param>
    /// <returns>Throws NotImplementedException as conversion back is not needed</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}