namespace TvGuide.Data;

public class RecordingJob
{
    public int Id { get; set; }

    public string Station { get; set; } = string.Empty;

    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}