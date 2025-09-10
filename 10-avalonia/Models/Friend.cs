using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaBasics.Models;

public partial class Friend : ObservableObject
{
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string firstName = string.Empty;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string lastName = string.Empty;
    
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(Age))]
    private DateTime dateOfBirth;

    public Friend(string firstName, string lastName, string email, string phone, DateTime dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        DateOfBirth = dateOfBirth;
    }

    public Friend() : this(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Today)
    {
    }

    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}
