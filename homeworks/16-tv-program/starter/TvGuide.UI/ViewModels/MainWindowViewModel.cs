using System.Threading.Tasks;
using TvGuide.Data;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace TvGuide.UI.ViewModels;

public partial class MainWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory) : ViewModelBase
{
    private readonly ApplicationDataContext dbContext = contextFactory.CreateDbContext();

    [RelayCommand]
    private async Task AddSampleData()
    {
        var sampleItem = new Greeting { GreetingText = "Hello, World!" };
        dbContext.Greetings.Add(sampleItem);
        await dbContext.SaveChangesAsync();
    }

    [RelayCommand]
    private async Task GetFirstSampleRow()
    {
        var firstGreeting = await dbContext.Greetings.FirstOrDefaultAsync();
        if (firstGreeting != null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Information",
                firstGreeting.GreetingText, ButtonEnum.Ok);
            await box.ShowAsync();
        }
    }
}
