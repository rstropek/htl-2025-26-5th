using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TvGuide.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TvGuide.UI.ViewModels;

// Classes for JSON deserialization
public class TvChannel
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("programs")]
    public List<TvProgram> Programs { get; set; } = new();
}

public class TvProgram
{
    [JsonPropertyName("startTime")]
    public string StartTime { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    [JsonPropertyName("markers")]
    public List<string>? Markers { get; set; }

    public string ChannelName { get; set; } = string.Empty;
}

// ViewModel classes for display
public class ProgramViewModel
{
    public string ChannelName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? MarkersDisplay { get; set; }
    public string? EndTime { get; set; }
}

public class RecordingJobViewModel
{
    public int Id { get; set; }
    public string Station { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ApplicationDataContext? dbContext;
    private readonly IConfiguration? configuration;
    private List<TvChannel> allChannels = new();
    private List<TvProgram> allPrograms = new();

    [ObservableProperty]
    private ObservableCollection<ProgramViewModel> orf1Programs = new();

    [ObservableProperty]
    private ObservableCollection<ProgramViewModel> orf2Programs = new();

    [ObservableProperty]
    private ObservableCollection<RecordingJobViewModel> recordingJobs = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    // Default constructor for design-time support
    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory, IConfiguration configuration)
    {
        dbContext = contextFactory.CreateDbContext();
        this.configuration = configuration;
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // Load TV Guide data from JSON
        var jsonPath = configuration?["TvGuideJsonPath"] ?? throw new InvalidOperationException("TvGuideJsonPath not configured in appsettings.json");
        
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"TV Guide JSON file not found at: {jsonPath}");
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        allChannels = JsonSerializer.Deserialize<List<TvChannel>>(json) ?? throw new InvalidOperationException("Failed to deserialize TV Guide JSON data");
        
        // Store all programs with their channel names
        foreach (var channel in allChannels)
        {
            foreach (var program in channel.Programs)
            {
                program.ChannelName = channel.Channel;
                allPrograms.Add(program);
            }
        }

        DisplayPrograms(allPrograms);

        // Load recording jobs from database
        await LoadRecordingJobsAsync();
    }

    private async Task LoadRecordingJobsAsync()
    {
        if (dbContext == null) return;

        var jobs = await dbContext.RecordingJobs.ToListAsync();
        RecordingJobs.Clear();
        foreach (var job in jobs)
        {
            RecordingJobs.Add(new RecordingJobViewModel
            {
                Id = job.Id,
                Station = job.Station,
                StartTime = job.StartTime,
                EndTime = job.EndTime,
                Title = job.Title
            });
        }
    }

    private void DisplayPrograms(List<TvProgram> programs)
    {
        var orf1 = programs.Where(p => p.ChannelName == "ORF 1").ToList();
        var orf2 = programs.Where(p => p.ChannelName == "ORF 2").ToList();

        Orf1Programs.Clear();
        foreach (var program in orf1)
        {
            Orf1Programs.Add(CreateProgramViewModel(program, orf1));
        }

        Orf2Programs.Clear();
        foreach (var program in orf2)
        {
            Orf2Programs.Add(CreateProgramViewModel(program, orf2));
        }
    }

    private ProgramViewModel CreateProgramViewModel(TvProgram program, List<TvProgram> channelPrograms)
    {
        // Calculate end time as the start time of the next program
        var currentIndex = channelPrograms.IndexOf(program);
        string? endTime = null;
        if (currentIndex >= 0 && currentIndex < channelPrograms.Count - 1)
        {
            endTime = channelPrograms[currentIndex + 1].StartTime;
        }

        return new ProgramViewModel
        {
            ChannelName = program.ChannelName,
            StartTime = program.StartTime,
            Title = program.Title,
            Subtitle = program.Subtitle,
            MarkersDisplay = program.Markers != null && program.Markers.Count > 0 
                ? string.Join(", ", program.Markers) 
                : null,
            EndTime = endTime
        };
    }

    [RelayCommand]
    private void Filter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            DisplayPrograms(allPrograms);
            return;
        }

        var searchLower = SearchText.ToLower();
        var filtered = allPrograms.Where(p =>
            p.Title.ToLower().Contains(searchLower) ||
            (p.Subtitle != null && p.Subtitle.ToLower().Contains(searchLower))
        ).ToList();

        DisplayPrograms(filtered);
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SearchText = string.Empty;
        DisplayPrograms(allPrograms);
    }

    [RelayCommand]
    private async Task Record(ProgramViewModel program)
    {
        if (dbContext == null) return;

        var recordingJob = new RecordingJob
        {
            Station = program.ChannelName,
            StartTime = program.StartTime,
            EndTime = program.EndTime ?? string.Empty,
            Title = program.Title
        };

        dbContext.RecordingJobs.Add(recordingJob);
        await dbContext.SaveChangesAsync();
        await LoadRecordingJobsAsync();
    }

    [RelayCommand]
    private async Task DeleteRecordingJob(RecordingJobViewModel job)
    {
        if (dbContext == null) return;

        var dbJob = await dbContext.RecordingJobs.FindAsync(job.Id);
        if (dbJob != null)
        {
            dbContext.RecordingJobs.Remove(dbJob);
            await dbContext.SaveChangesAsync();
            await LoadRecordingJobsAsync();
        }
    }
}
