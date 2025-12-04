namespace WebApi;

public static class DemoEndpoints
{
    private static readonly List<Customer> Customers =
    [
        new(1, "Alice", new DateOnly(1985, 1, 15), 5, 1234.56m, true),
        new(2, "Bob", new DateOnly(1990, 6, 30), 3, 7890.12m, true),
        new(3, "Charlie", new DateOnly(1975, 12, 5), 4, 3456.78m, false),
        new(4, "Diana", new DateOnly(2000, 3, 20), 2, 9012.34m, true),
        new(5, "Ethan", new DateOnly(1988, 9, 10), 1, 5678.90m, false)
    ];

    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        // Demonstrates an endpoint returning a string
        app.MapGet("/ping", () => "pong")
            .WithDescription("A simple ping endpoint to check if the service is running.");

        // Get all customers
        app.MapGet("/customers", () => Customers.OrderBy(c => c.Id))
            .WithDescription("Get all customers ordered by ID")
            .Produces<List<Customer>>(StatusCodes.Status200OK);

        // Get customer by ID
        app.MapGet("/customers/{id}", (int id) =>
        {
            var customer = Customers.FirstOrDefault(c => c.Id == id);
            return customer is not null ? Results.Ok(customer) : Results.NotFound();
        })
        .WithDescription("Get a customer by ID")
        .Produces<Customer>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Insert (create) a new customer
        app.MapPost("/customers", (Customer customer) =>
        {
            // Check if ID already exists
            if (Customers.Any(c => c.Id == customer.Id))
            {
                return Results.Conflict(new { error = "Customer with this ID already exists" });
            }

            Customers.Add(customer);
            return Results.Created($"/customers/{customer.Id}", customer);
        })
        .WithDescription("Create a new customer")
        .Produces<Customer>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict);

        // Update (PATCH) a customer
        app.MapPatch("/customers/{id}", (int id, CustomerPatchDto patch) =>
        {
            var customer = Customers.FirstOrDefault(c => c.Id == id);
            if (customer is null)
            {
                return Results.NotFound();
            }

            // Remove the old customer and add the updated one
            Customers.Remove(customer);
            var updated = customer with
            {
                Name = patch.Name ?? customer.Name,
                DateOfBirth = patch.DateOfBirth ?? customer.DateOfBirth,
                CustomerValue = patch.CustomerValue ?? customer.CustomerValue,
                Revenue = patch.Revenue ?? customer.Revenue,
                IsActive = patch.IsActive ?? customer.IsActive
            };
            Customers.Add(updated);

            return Results.Ok(updated);
        })
        .WithDescription("Update a customer (partial update)")
        .Produces<Customer>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Delete a customer
        app.MapDelete("/customers/{id}", (int id) =>
        {
            var customer = Customers.FirstOrDefault(c => c.Id == id);
            if (customer is null)
            {
                return Results.NotFound();
            }

            Customers.Remove(customer);
            return Results.NoContent();
        })
        .WithDescription("Delete a customer by ID")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

record Customer(
    int Id, 
    string Name, 
    DateOnly DateOfBirth, 
    int CustomerValue, 
    decimal Revenue, 
    bool IsActive);

record CustomerPatchDto(
    string? Name = null,
    DateOnly? DateOfBirth = null,
    int? CustomerValue = null, 
    decimal? Revenue = null, 
    bool? IsActive = null);
