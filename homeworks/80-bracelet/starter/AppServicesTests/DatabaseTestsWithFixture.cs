using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace AppServicesTests;

public class DatabaseTestsWithClassFixture(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CanAddAndRetrieveOrder()
    {
        int orderId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var order = new Order
            {
                CustomerName = "Alice",
                CustomerAddress = "123 Main St",
                TotalCosts = 3.50m,
                OrderDate = DateTime.UtcNow,
                OrderItems =
                [
                    new OrderItem { BraceletData = "H|pink|I", Costs = 2.50m },
                    new OrderItem { BraceletData = "A", Costs = 1.00m }
                ]
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            orderId = order.Id;
        }

        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .FirstAsync(o => o.Id == orderId);
            Assert.Equal("Alice", order.CustomerName);
            Assert.Equal(2, order.OrderItems.Count);
        }
    }

    [Fact]
    public async Task CanDeleteOrder()
    {
        int orderId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var order = new Order
            {
                CustomerName = "Bob",
                CustomerAddress = "456 Oak Ave",
                TotalCosts = 1.00m,
                OrderDate = DateTime.UtcNow,
                OrderItems = [new OrderItem { BraceletData = "A", Costs = 1.00m }]
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            orderId = order.Id;
        }

        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var order = await context.Orders.FindAsync(orderId);
            Assert.NotNull(order);
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var order = await context.Orders.FindAsync(orderId);
            Assert.Null(order);
        }
    }

    [Fact]
    public async Task DecimalCostsStoreCorrectly()
    {
        int orderId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var order = new Order
            {
                CustomerName = "Charlie",
                CustomerAddress = "789 Elm St",
                TotalCosts = 12.50m,
                OrderDate = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            orderId = order.Id;
        }

        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var order = await context.Orders.FindAsync(orderId);
            Assert.NotNull(order);
            Assert.Equal(12.50m, order.TotalCosts, 2);
        }
    }
}
