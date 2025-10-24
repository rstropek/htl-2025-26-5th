using AppServices;

namespace WebApi;

public static class DemoEndpoints
{
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        // Demonstrates an endpoint returning a string
        app.MapGet("/ping", () => "pong")
            .WithDescription("A simple ping endpoint to check if the service is running.");

        // Demonstrates an endpoint interacting with the database
        // and returning a list of objects.
        app.MapGet("/dummies", (ApplicationDataContext db) => db.Dummies)
            .WithDescription("Gets all dummy records from the database.");

        // Demonstrates an endpoint that uses a service to perform some logic.
        // Receives an object, modifies it using the service, and returns the 
        // modified object.
        app.MapPost("/dummy-logic", async (ApplicationDataContext db, Dummy dummyToChange, IDummyLogic logic) =>
        {
            logic.IncrementDecimal(dummyToChange, 1.5m);
            return Results.Ok(dummyToChange);
        })
        .Produces<Dummy>(StatusCodes.Status200OK)
        .WithDescription("Increments the DecimalProperty of the provided Dummy object by 1.5 using the DummyLogic service.");

        return app;
    }
}