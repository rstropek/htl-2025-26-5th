using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace AppServicesTests;

public class DatabaseTestsWithClassFixture(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task TravelEntity_Crud_Selects_And_UtcAndRealPrecision()
    {
        var startUtc = new DateTimeOffset(2026, 01, 20, 10, 30, 00, TimeSpan.Zero);
        var endUtc = new DateTimeOffset(2026, 01, 21, 11, 45, 30, TimeSpan.Zero);

        // Arrange: delete all rows
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            await DeleteAllTravelsAndReimbursements(context);
        }

        // Act: add
        int travelId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var travel = new TravelEntity
            {
                Start = startUtc,
                End = endUtc,
                TravelerName = "Ada Lovelace",
                Purpose = "Conference",
                Mileage = 123.456m,
                PerDiem = 78.901m,
                Expenses = 10.005m
            };
            context.Travels.Add(travel);
            await context.SaveChangesAsync();
            travelId = travel.Id;
        }

        // Assert: select by id and select all
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byId = await context.Travels.FindAsync(travelId);
            Assert.NotNull(byId);

            Assert.Equal(TimeSpan.Zero, byId.Start.Offset);
            Assert.Equal(TimeSpan.Zero, byId.End.Offset);
            Assert.Equal(startUtc.UtcDateTime, byId.Start.UtcDateTime);
            Assert.Equal(endUtc.UtcDateTime, byId.End.UtcDateTime);

            Assert.Equal("Ada Lovelace", byId.TravelerName);
            Assert.Equal("Conference", byId.Purpose);

            // Stored as SQLite REAL via double conversion; compare with precision tolerance.
            Assert.Equal(123.456m, byId.Mileage, 3);
            Assert.Equal(78.901m, byId.PerDiem, 3);
            Assert.Equal(10.005m, byId.Expenses, 3);

            var all = await context.Travels.ToListAsync();
            Assert.Single(all);
            Assert.Equal(travelId, all[0].Id);
        }

        // Act: update
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var travel = await context.Travels.FindAsync(travelId);
            Assert.NotNull(travel);
            travel.Purpose = "Updated Purpose";
            travel.Mileage = 222.222m;
            await context.SaveChangesAsync();
        }

        // Assert: updated values visible via id and all
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byId = await context.Travels.FindAsync(travelId);
            Assert.NotNull(byId);
            Assert.Equal("Updated Purpose", byId.Purpose);
            Assert.Equal(222.222m, byId.Mileage, 3);

            var all = await context.Travels.ToListAsync();
            Assert.Single(all);
            Assert.Equal("Updated Purpose", all[0].Purpose);
        }

        // Act: delete
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var travel = await context.Travels.FindAsync(travelId);
            Assert.NotNull(travel);
            context.Travels.Remove(travel);
            await context.SaveChangesAsync();
        }

        // Assert: deleted
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byId = await context.Travels.FindAsync(travelId);
            Assert.Null(byId);
            var all = await context.Travels.ToListAsync();
            Assert.Empty(all);
        }
    }

    [Fact]
    public async Task DriveWithPrivateCarReimbursementEntity_Crud_Selects()
    {
        // Arrange: delete all rows
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            await DeleteAllTravelsAndReimbursements(context);
        }

        // Add a travel (required FK)
        int travelId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var travel = new TravelEntity
            {
                Start = new DateTimeOffset(2026, 01, 20, 08, 00, 00, TimeSpan.Zero),
                End = new DateTimeOffset(2026, 01, 20, 18, 00, 00, TimeSpan.Zero),
                TravelerName = "Test Traveler",
                Purpose = "Trip",
                Mileage = 0m,
                PerDiem = 0m,
                Expenses = 0m
            };
            context.Travels.Add(travel);
            await context.SaveChangesAsync();
            travelId = travel.Id;
        }

        // Act: add reimbursement
        int reimbursementId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var reimbursement = new DriveWithPrivateCarReimbursementEntity
            {
                TravelId = travelId,
                Description = "Drive to customer",
                KM = 42
            };
            context.TravelReimbursements.Add(reimbursement);
            await context.SaveChangesAsync();
            reimbursementId = reimbursement.Id;
        }

        // Assert: select by id and select all
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            var byId = Assert.IsType<DriveWithPrivateCarReimbursementEntity>(byIdBase);
            Assert.Equal(travelId, byId.TravelId);
            Assert.Equal("Drive to customer", byId.Description);
            Assert.Equal(42, byId.KM);

            var all = await context.TravelReimbursements.OfType<DriveWithPrivateCarReimbursementEntity>().ToListAsync();
            Assert.Single(all);
            Assert.Equal(reimbursementId, all[0].Id);
        }

        // Act: update reimbursement
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            var reimbursement = Assert.IsType<DriveWithPrivateCarReimbursementEntity>(byIdBase);
            reimbursement.Description = "Updated description";
            reimbursement.KM = 99;
            await context.SaveChangesAsync();
        }

        // Assert: updated
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            var byId = Assert.IsType<DriveWithPrivateCarReimbursementEntity>(byIdBase);
            Assert.Equal("Updated description", byId.Description);
            Assert.Equal(99, byId.KM);

            var all = await context.TravelReimbursements.OfType<DriveWithPrivateCarReimbursementEntity>().ToListAsync();
            Assert.Single(all);
            Assert.Equal("Updated description", all[0].Description);
        }

        // Act: delete reimbursement
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            context.TravelReimbursements.Remove(byIdBase);
            await context.SaveChangesAsync();
        }

        // Assert: deleted (and cleanup travel)
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byId = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.Null(byId);
            var all = await context.TravelReimbursements.OfType<DriveWithPrivateCarReimbursementEntity>().ToListAsync();
            Assert.Empty(all);

            var travel = await context.Travels.FindAsync(travelId);
            Assert.NotNull(travel);
            context.Travels.Remove(travel);
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task ExpenseReimbursementEntity_Crud_Selects()
    {
        // Arrange: delete all rows
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            await DeleteAllTravelsAndReimbursements(context);
        }

        // Add a travel (required FK)
        int travelId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var travel = new TravelEntity
            {
                Start = new DateTimeOffset(2026, 01, 20, 08, 00, 00, TimeSpan.Zero),
                End = new DateTimeOffset(2026, 01, 20, 18, 00, 00, TimeSpan.Zero),
                TravelerName = "Test Traveler",
                Purpose = "Trip",
                Mileage = 0m,
                PerDiem = 0m,
                Expenses = 0m
            };
            context.Travels.Add(travel);
            await context.SaveChangesAsync();
            travelId = travel.Id;
        }

        // Act: add reimbursement
        int reimbursementId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var reimbursement = new ExpenseReimbursementEntity
            {
                TravelId = travelId,
                Description = "Hotel",
                Amount = 199
            };
            context.TravelReimbursements.Add(reimbursement);
            await context.SaveChangesAsync();
            reimbursementId = reimbursement.Id;
        }

        // Assert: select by id and select all
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            var byId = Assert.IsType<ExpenseReimbursementEntity>(byIdBase);
            Assert.Equal(travelId, byId.TravelId);
            Assert.Equal("Hotel", byId.Description);
            Assert.Equal(199, byId.Amount);

            var all = await context.TravelReimbursements.OfType<ExpenseReimbursementEntity>().ToListAsync();
            Assert.Single(all);
            Assert.Equal(reimbursementId, all[0].Id);
        }

        // Act: update
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            var reimbursement = Assert.IsType<ExpenseReimbursementEntity>(byIdBase);
            reimbursement.Description = "Updated Hotel";
            reimbursement.Amount = 250;
            await context.SaveChangesAsync();
        }

        // Assert: updated
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            var byId = Assert.IsType<ExpenseReimbursementEntity>(byIdBase);
            Assert.Equal("Updated Hotel", byId.Description);
            Assert.Equal(250, byId.Amount);

            var all = await context.TravelReimbursements.OfType<ExpenseReimbursementEntity>().ToListAsync();
            Assert.Single(all);
            Assert.Equal("Updated Hotel", all[0].Description);
        }

        // Act: delete
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byIdBase = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.NotNull(byIdBase);
            context.TravelReimbursements.Remove(byIdBase);
            await context.SaveChangesAsync();
        }

        // Assert: deleted (and cleanup travel)
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var byId = await context.TravelReimbursements.FindAsync(reimbursementId);
            Assert.Null(byId);
            var all = await context.TravelReimbursements.OfType<ExpenseReimbursementEntity>().ToListAsync();
            Assert.Empty(all);

            var travel = await context.Travels.FindAsync(travelId);
            Assert.NotNull(travel);
            context.Travels.Remove(travel);
            await context.SaveChangesAsync();
        }
    }

    private static async Task DeleteAllTravelsAndReimbursements(ApplicationDataContext context)
    {
        var reimbursements = await context.TravelReimbursements.ToListAsync();
        context.TravelReimbursements.RemoveRange(reimbursements);

        var travels = await context.Travels.ToListAsync();
        context.Travels.RemoveRange(travels);

        await context.SaveChangesAsync();
    }
}
