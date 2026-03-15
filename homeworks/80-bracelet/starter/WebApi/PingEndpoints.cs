namespace WebApi;

/// <summary>
/// Simple health-check endpoint that can be used to verify the API is running.
/// </summary>
public static class PingEndpoints
{
    /// <summary>
    /// Maps the <c>GET /api/ping</c> endpoint which returns a <see cref="PingResultDto"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapPingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/ping", () => Results.Ok(new PingResultDto("pong", DateTime.UtcNow)))
            .Produces<PingResultDto>();

        return app;
    }
}

/// <summary>
/// Response returned by the ping endpoint.
/// </summary>
/// <param name="Message">The response message (always "pong").</param>
/// <param name="Timestamp">The server's current UTC date and time.</param>
public record PingResultDto(string Message, DateTime Timestamp);
