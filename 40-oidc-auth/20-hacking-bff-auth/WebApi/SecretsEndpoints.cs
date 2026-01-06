using AppServices;
using Microsoft.AspNetCore.Mvc;

namespace WebApi;

public static class SecretsEndpoints
{
    public static IEndpointRouteBuilder MapSecretsEndpoints(this IEndpointRouteBuilder app)
    {
        // Get all secrets
        app.MapGet("/secrets", (ApplicationDataContext db) =>
            db.Secrets.Select(s => new SecretsDto(s.Id, s.ConnectionString))
        )
        .WithDescription("Gets all secrets records from the database.");

        // Get a specific secret by ID
        app.MapGet("/secrets/{id}", async (int id, ApplicationDataContext db) =>
        {
            var secret = await db.Secrets.FindAsync(id);
            return secret is not null ? Results.Ok(new SecretsDto(secret.Id, secret.ConnectionString)) : Results.NotFound();
        })
        .Produces<SecretsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Gets a specific secret by ID.");

        // Create a new secret
        app.MapPost("/secrets", async (ApplicationDataContext db, SecretsCreateDto secretsDto) =>
        {
            var secret = new Secrets { ConnectionString = secretsDto.ConnectionString };
            db.Secrets.Add(secret);
            await db.SaveChangesAsync();
            return Results.Created($"/secrets/{secret.Id}", new SecretsDto(secret.Id, secret.ConnectionString));
        })
        .Produces<SecretsDto>(StatusCodes.Status201Created)
        .WithDescription("Creates a new secret record.");

        // Update an existing secret
        app.MapPut("/secrets/{id}", async (int id, ApplicationDataContext db, SecretsUpdateDto secretsDto) =>
        {
            if (string.IsNullOrEmpty(secretsDto.ConnectionString))
            {
                return Results.BadRequest("Connection string is required.");
            }

            var existingSecret = await db.Secrets.FindAsync(id);
            if (existingSecret is null)
            {
                return Results.NotFound();
            }

            existingSecret.ConnectionString = secretsDto.ConnectionString;
            await db.SaveChangesAsync();
            return Results.Ok(new SecretsDto(existingSecret.Id, existingSecret.ConnectionString));
        })
        .Produces<SecretsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Updates an existing secret record.");

        // To make testing simpler, provide a way to update a secret using GET
        app.MapGet("/secrets/{id}/update", async (int id, ApplicationDataContext db, [FromQuery] string connectionString) =>
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return Results.BadRequest("Connection string is required.");
            }

            var existingSecret = await db.Secrets.FindAsync(id);
            if (existingSecret is null)
            {
                return Results.NotFound();
            }

            existingSecret.ConnectionString = connectionString;
            await db.SaveChangesAsync();
            return Results.Ok(new SecretsDto(existingSecret.Id, existingSecret.ConnectionString));
        })
        .Produces<SecretsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Updates an existing secret record (via GET).");

        // Delete a secret
        app.MapDelete("/secrets/{id}", async (int id, ApplicationDataContext db) =>
        {
            var secret = await db.Secrets.FindAsync(id);
            if (secret is null)
            {
                return Results.NotFound();
            }

            db.Secrets.Remove(secret);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Deletes a secret record by ID.");

        return app;
    }
}

public record SecretsDto(int Id, string ConnectionString);

public record SecretsCreateDto(string ConnectionString);

public record SecretsUpdateDto(string ConnectionString);