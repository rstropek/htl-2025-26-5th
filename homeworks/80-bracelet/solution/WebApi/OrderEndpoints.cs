using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders", async (decimal? minTotalCosts, ApplicationDataContext db) =>
        {
            var query = db.Orders.AsQueryable();
            if (minTotalCosts.HasValue)
            {
                query = query.Where(o => o.TotalCosts >= minTotalCosts.Value);
            }
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListDto(o.Id, o.CustomerName, o.OrderDate, o.TotalCosts, o.OrderItems.Count))
                .ToListAsync();
            return Results.Ok(orders);
        })
        .Produces<List<OrderListDto>>();

        app.MapGet("/api/orders/{id}", async (int id, ApplicationDataContext db) =>
        {
            var order = await db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
            {
                return Results.NotFound();
            }

            var dto = new OrderDetailDto(
                order.Id,
                order.CustomerName,
                order.CustomerAddress,
                order.OrderDate,
                order.TotalCosts,
                order.OrderItems.Select(oi => new OrderItemDto(oi.Id, oi.BraceletData, oi.Costs)).ToList()
            );
            return Results.Ok(dto);
        })
        .Produces<OrderDetailDto>()
        .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/api/orders", async (CreateOrderDto input, ApplicationDataContext db, IBraceletSerializer serializer) =>
        {
            if (string.IsNullOrWhiteSpace(input.CustomerName))
            {
                return Results.BadRequest("Customer name is required.");
            }

            if (string.IsNullOrWhiteSpace(input.CustomerAddress))
            {
                return Results.BadRequest("Customer address is required.");
            }

            if (input.Bracelets is null || input.Bracelets.Count == 0)
            {
                return Results.BadRequest("At least one bracelet is required.");
            }

            var orderItems = new List<OrderItem>();
            decimal totalCosts = 0;

            foreach (var braceletData in input.Bracelets)
            {
                var validationResult = serializer.Parse(braceletData, out var bracelet);
                if (validationResult != BraceletValidationResult.Ok)
                {
                    return Results.BadRequest($"Invalid bracelet: {validationResult}");
                }

                totalCosts += bracelet!.Cost;
                orderItems.Add(new OrderItem { BraceletData = bracelet.Data, Costs = bracelet.Cost });
            }

            var order = new Order
            {
                CustomerName = input.CustomerName,
                CustomerAddress = input.CustomerAddress,
                TotalCosts = totalCosts,
                OrderDate = DateTime.UtcNow,
                OrderItems = orderItems
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            return Results.Created($"/api/orders/{order.Id}", new OrderDetailDto(
                order.Id,
                order.CustomerName,
                order.CustomerAddress,
                order.OrderDate,
                order.TotalCosts,
                order.OrderItems.Select(oi => new OrderItemDto(oi.Id, oi.BraceletData, oi.Costs)).ToList()
            ));
        })
        .Produces<OrderDetailDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/api/validate-bracelet", (ValidateBraceletDto input, IBraceletSerializer serializer) =>
        {
            var result = serializer.Parse(input.BraceletData, out var bracelet);
            if (result != BraceletValidationResult.Ok)
            {
                return Results.Ok(new BraceletValidationResultDto(result.ToString(), false, null));
            }

            return Results.Ok(new BraceletValidationResultDto(null, bracelet!.HasMixedColors, bracelet.Cost));
        })
        .Produces<BraceletValidationResultDto>();

        return app;
    }
}

public record OrderListDto(int Id, string CustomerName, DateTime OrderDate, decimal TotalCosts, int NumberOfBracelets);
public record OrderDetailDto(int Id, string CustomerName, string CustomerAddress, DateTime OrderDate, decimal TotalCosts, List<OrderItemDto> OrderItems);
public record OrderItemDto(int Id, string BraceletData, decimal Costs);
public record CreateOrderDto(string CustomerName, string CustomerAddress, List<string> Bracelets);
public record ValidateBraceletDto(string BraceletData);
public record BraceletValidationResultDto(string? Error, bool MixedColorsWarning, decimal? Cost);
