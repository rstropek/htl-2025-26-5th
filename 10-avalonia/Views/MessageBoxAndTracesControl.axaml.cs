using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AvaloniaBasics.Views;

// In this file, you see various forms of message boxes and trace/debug messages.
// Note that avalonia does not have built-in message boxes, so we use a third-party library (MessageBox.Avalonia).
// TODO: Make yourself familiar with message boxes (https://github.com/AvaloniaCommunity/MessageBox.Avalonia)

public partial class MessageBoxAndTracesControl : UserControl
{
    public MessageBoxAndTracesControl()
    {
        InitializeComponent();
    }

    private async void ShowMessageButton_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Information",
            "This is a simple message box!",
            ButtonEnum.Ok);

        // ShowAsync displays the message box, choosing the presentation style—popup or window—according to the application type:
        // - SingleViewApplicationLifetime (used in mobile or browser environments): shows as a popup
        // - ClassicDesktopStyleApplicationLifetime (desktop apps): shows as a window
        await box.ShowAsync();
    }

    private async void ShowPopupButton_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Information",
            "This is a simple message box!",
            ButtonEnum.Ok);

        // Find the parent window to show as popup
        var parentWindow = this.FindAncestorOfType<Window>();
        if (parentWindow != null)
        {
            // Show explicitly as popup
            await box.ShowAsPopupAsync(parentWindow);
        }
        else
        {
            await box.ShowAsync();
        }
    }

    private async void ShowYesNoButton_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Question",
            "Do you want to continue?",
            ButtonEnum.YesNo);
        var result = await box.ShowAsync();

        var responseBox = MessageBoxManager.GetMessageBoxStandard("Result",
            $"You clicked: {result}",
            ButtonEnum.Ok);
        // Show explicitly as window
        await responseBox.ShowWindowAsync();
    }

    private void WriteTraceButton_Click(object? sender, RoutedEventArgs e)
    {
        // Trace messsages appear in the debug output in VSCode.
        // TODO: Debug this app, trigger this button, and find the output.
        Trace.WriteLine("Trace message written from MessageBoxAndTracesControl button click");
        Debug.WriteLine("Debug message written from MessageBoxAndTracesControl button click");
    }
}
