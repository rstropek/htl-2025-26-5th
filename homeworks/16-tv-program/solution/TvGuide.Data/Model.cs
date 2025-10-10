namespace TvGuide.Data;

// Add your model classes here
// IMPORTANT: Read https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
//            to learn about SQLite limitations

public class RecordingJob
{
    public int Id { get; set; }

    public string Station { get; set; } = string.Empty;

    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}