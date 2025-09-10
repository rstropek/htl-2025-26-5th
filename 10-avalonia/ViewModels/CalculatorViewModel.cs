using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaBasics.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    // First, we define two properties receiving the input numbers.
    // The user will enter these numbers in the UI.
    public double FirstNumber { get; set; }
    public double SecondNumber { get; set; }

    // Next, we define properties for the selected operator and the result.
    // The values of these members will be updated **in code**. Therefore,
    // we use [ObservableProperty] to generate the boilerplate code for
    // INotifyPropertyChanged automatically.
    // This "magic" is done by the source generators in the
    // CommunityToolkit.Mvvm package. Find more at
    // https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/overview
    [ObservableProperty] private string selectedOperator = "+";

    [ObservableProperty] private double result;

    [ObservableProperty] private string resultText = "0";

    // Note the use of ObservableCollection here. It is a collection
    // that notifies the UI when its content changes.
    public ObservableCollection<string> Operators { get; } = ["+", "-", "*", "/"];

    // Here we use the [RelayCommand] attribute to generate
    // the ICommand implementation for us. ICommand is used
    // for binding buttons in the UI to methods in the view model.
    // In this case, we also specify a method that determines
    // whether the command can be executed or not.
    // TODO: Make yourself familiar with RelayCommand
    //       (https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/relaycommand)
    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        Result = SelectedOperator switch
        {
            "+" => FirstNumber + SecondNumber,
            "-" => FirstNumber - SecondNumber,
            "*" => FirstNumber * SecondNumber,
            "/" => FirstNumber / SecondNumber,
            _ => throw new InvalidOperationException("Unknown operator")
        };

        ResultText = Result.ToString("F2");
    }

    private bool CanCalculate() => SelectedOperator != "/" || SecondNumber != 0;

    [RelayCommand]
    private void Clear()
    {
        FirstNumber = 0;
        SecondNumber = 0;
        SelectedOperator = "+";
        Result = 0;
        ResultText = "0";
    }
}
