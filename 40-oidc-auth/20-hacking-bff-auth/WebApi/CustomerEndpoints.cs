using AppServices;

namespace WebApi;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        // Get all customers
        app.MapGet("/customers", (ApplicationDataContext db) =>
            db.Customers.Select(c => new CustomerDto(c.Id, c.Name))
        )
        .WithDescription("Gets all customer records from the database.");

        // Get a specific customer by ID
        app.MapGet("/customers/{id}", async (int id, ApplicationDataContext db) =>
        {
            var customer = await db.Customers.FindAsync(id);
            return customer is not null ? Results.Ok(new CustomerDto(customer.Id, customer.Name)) : Results.NotFound();
        })
        .Produces<CustomerDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Gets a specific customer by ID.");

        // Create a new customer
        app.MapPost("/customers", async (ApplicationDataContext db, CustomerCreateDto customerDto) =>
        {
            var customer = new Customer { Name = customerDto.Name };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
            return Results.Created($"/customers/{customer.Id}", new CustomerDto(customer.Id, customer.Name));
        })
        .Produces<CustomerDto>(StatusCodes.Status201Created)
        .WithDescription("Creates a new customer record.");

        // Update an existing customer
        app.MapPut("/customers/{id}", async (int id, ApplicationDataContext db, CustomerUpdateDto customerDto) =>
        {
            if (string.IsNullOrEmpty(customerDto.Name))
            {
                return Results.BadRequest("Customer name is required.");
            }

            var existingCustomer = await db.Customers.FindAsync(id);
            if (existingCustomer is null)
            {
                return Results.NotFound();
            }

            existingCustomer.Name = customerDto.Name;
            await db.SaveChangesAsync();
            return Results.Ok(new CustomerDto(existingCustomer.Id, existingCustomer.Name));
        })
        .Produces<CustomerDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Updates an existing customer record.");

        // Delete a customer
        app.MapDelete("/customers/{id}", async (int id, ApplicationDataContext db) =>
        {
            var customer = await db.Customers.FindAsync(id);
            if (customer is null)
            {
                return Results.NotFound();
            }

            db.Customers.Remove(customer);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Deletes a customer record by ID.");

        return app;
    }
}

public record CustomerDto(int Id, string Name);

public record CustomerCreateDto(string Name);

public record CustomerUpdateDto(string Name);