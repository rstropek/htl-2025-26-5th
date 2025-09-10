using System.Collections.ObjectModel;
using AvaloniaBasics.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaBasics.ViewModels;

public partial class FriendListViewModel : ViewModelBase
{
    // ObservableCollection to hold all friends. Note that here
    // it is particularly important to use an ObservableCollection
    // so that the UI can automatically update when items are added or removed.
    public ObservableCollection<Friend> Friends { get; } = [];

    // Lists for generating random friends
    private readonly string[] firstNames = ["John", "Jane", "Mike", "Sarah", "David", "Emma", "Chris", "Lisa", "Alex", "Maria", "Tom", "Anna"];
    private readonly string[] lastNames = ["Smith", "Johnson", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White"];
    private readonly string[] domains = ["gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "example.com"];

    public FriendListViewModel()
    {
        // Add some initial sample data
        AddSampleFriends();
    }

    // Command to add a random friend
    [RelayCommand]
    private void AddRandomFriend()
    {
        var firstName = Random.Shared.GetItems(firstNames, 1)[0];
        var lastName = Random.Shared.GetItems(lastNames, 1)[0];
        var email = $"{firstName.ToLower()}.{lastName.ToLower()}@{Random.Shared.GetItems(domains, 1)[0]}";
        var phone = $"+1-{Random.Shared.Next(100, 999)}-{Random.Shared.Next(100, 999)}-{Random.Shared.Next(1000, 9999)}";
        var dateOfBirth = DateTime.Today.AddYears(-Random.Shared.Next(18, 65)).AddDays(-Random.Shared.Next(0, 365));

        Friends.Add(new Friend(firstName, lastName, email, phone, dateOfBirth));
    }

    // Command to delete a friend
    [RelayCommand]
    private void DeleteFriend(Friend? friend)
    {
        if (friend != null)
        {
            Friends.Remove(friend);
        }
    }

    // Command to clear all friends
    [RelayCommand]
    private void ClearAll()
    {
        Friends.Clear();
    }

    private void AddSampleFriends()
    {
        Friends.Add(new Friend("Alice", "Cooper", "alice.cooper@example.com", "+1-555-0101", new DateTime(1990, 3, 15)));
        Friends.Add(new Friend("Bob", "Builder", "bob.builder@example.com", "+1-555-0102", new DateTime(1985, 7, 22)));
        Friends.Add(new Friend("Charlie", "Chaplin", "charlie.chaplin@example.com", "+1-555-0103", new DateTime(1992, 11, 8)));
    }
}
