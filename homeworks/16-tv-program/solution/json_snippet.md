## DTOs for JSON Deserialization

```cs
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
```

## Serialization Example

```cs
// jsonPath contains the path to the JSON file
var json = await File.ReadAllTextAsync(jsonPath);
allChannels = JsonSerializer.Deserialize<List<TvChannel>>(json) ?? throw new InvalidOperationException("Failed to deserialize TV Guide JSON data");
```
