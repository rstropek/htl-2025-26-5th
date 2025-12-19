using AppServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class WishlistEndpoints
{
    public static IEndpointRouteBuilder MapWishlistEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/gift-categories", GetGiftCategories)
            .Produces<List<string>>(StatusCodes.Status200OK)
            .WithDescription("Retrieves all gift categories (public, no PIN required). ");

        app.MapPost("/verify-pin/{name}", VerifyPin)
            .Produces<VerifyPinResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Verifies a wishlist PIN and returns whether it belongs to a parent or child.");

        app.MapPost("/wishlist/{name}/items", GetWishlistItems)
            .Produces<List<WishlistItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithDescription("Retrieves all wishlist items (parent only).");

        app.MapPost("/wishlist/{name}/items/{itemId:int}/mark-as-bought", MarkItemAsBought)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Marks a wishlist item as bought/unbought (parent only). ");

        app.MapDelete("/wishlist/{name}/items/{itemId:int}", DeleteItem)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Deletes an item from a wishlist (parent only). ");

        app.MapPost("/wishlist/{name}/items/add", AddItem)
            .Produces<AddItemResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDescription("Adds a new item to the wishlist (parent or child). Bought is always initialized as false.");

        return app;
    }

    private static async Task<IResult> GetGiftCategories(ApplicationDataContext db)
    {
        var categories = await db.GiftCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();

        return Results.Ok(categories);
    }

    private static async Task<IResult> VerifyPin(ApplicationDataContext db, string name, VerifyPinRequestDto request)
    {
        var wishlist = await db.Wishlists.AsNoTracking().FirstOrDefaultAsync(w => w.Name == name);
        if (wishlist is null) { return Results.Unauthorized(); }

        var role = GetRole(wishlist, request.Pin);
        return role switch
        {
            PinRole.Parent => Results.Ok(new VerifyPinResponseDto("parent")),
            PinRole.Child => Results.Ok(new VerifyPinResponseDto("child")),
            _ => Results.Unauthorized()
        };
    }

    private static async Task<IResult> GetWishlistItems(ApplicationDataContext db, string name, AuthRequestDto request)
    {
        var (wishlist, role) = await AuthenticateAsync(db, name, request.Pin);
        if (wishlist is null) { return Results.Unauthorized(); }

        if (role != PinRole.Parent) { return Results.StatusCode(StatusCodes.Status403Forbidden); }

        var items = await db.WishlistItems
            .AsNoTracking()
            .Where(i => i.WishlistId == wishlist.Id)
            .Include(i => i.Category)
            .OrderBy(i => i.Id)
            .Select(i => new WishlistItemDto(i.Id, i.ItemName, i.Category.Name, i.Bought))
            .ToListAsync();

        return Results.Ok(items);
    }

    private static async Task<IResult> AddItem(ApplicationDataContext db, string name, AddItemRequestDto request)
    {
        var (wishlist, role) = await AuthenticateAsync(db, name, request.Pin);
        if (wishlist is null) { return Results.Unauthorized(); }

        if (string.IsNullOrWhiteSpace(request.ItemName) || request.ItemName.Length > 100)
        {
            return Results.BadRequest("ItemName is required and must be <= 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Category) || request.Category.Length > 50)
        {
            return Results.BadRequest("Category is required and must be <= 50 characters.");
        }

        var category = await db.GiftCategories.FirstOrDefaultAsync(c => c.Name == request.Category);
        category ??= new GiftCategory { Name = request.Category };

        var item = new WishlistItem
        {
            WishlistId = wishlist.Id,
            Category = category,
            ItemName = request.ItemName,
            Bought = false
        };
        db.WishlistItems.Add(item);
        await db.SaveChangesAsync();

        var response = new AddItemResponseDto(item.Id, item.ItemName, request.Category, item.Bought);
        return Results.Created($"/wishlist/{name}/items/{item.Id}", response);
    }

    private static async Task<IResult> MarkItemAsBought(ApplicationDataContext db, string name, int itemId, MarkAsBoughtRequestDto request)
    {
        var (wishlist, role) = await AuthenticateAsync(db, name, request.Pin);
        if (wishlist is null) { return Results.Unauthorized(); }

        if (role != PinRole.Parent) { return Results.StatusCode(StatusCodes.Status403Forbidden); }

        var item = await db.WishlistItems.FirstOrDefaultAsync(i => i.Id == itemId && i.WishlistId == wishlist.Id);
        if (item is null)
        {
            return Results.NotFound();
        }

        item.Bought = request.Bought;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteItem(ApplicationDataContext db, string name, int itemId, [FromBody] AuthRequestDto request)
    {
        var (wishlist, role) = await AuthenticateAsync(db, name, request.Pin);
        if (wishlist is null) { return Results.Unauthorized(); }

        if (role != PinRole.Parent) { return Results.StatusCode(StatusCodes.Status403Forbidden); }

        var item = await db.WishlistItems.FirstOrDefaultAsync(i => i.Id == itemId && i.WishlistId == wishlist.Id);
        if (item is null)
        {
            return Results.NotFound();
        }

        db.WishlistItems.Remove(item);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<(Wishlist? wishlist, PinRole role)> AuthenticateAsync(ApplicationDataContext db, string name, string pin)
    {
        var wishlist = await db.Wishlists.AsNoTracking().FirstOrDefaultAsync(w => w.Name == name);
        if (wishlist is null) { return (null, PinRole.None); }

        var role = GetRole(wishlist, pin);
        if (role == PinRole.None) { return (null, PinRole.None); }

        return (wishlist, role);
    }

    private static PinRole GetRole(Wishlist wishlist, string pin)
    {
        if (string.Equals(wishlist.ParentPin, pin, StringComparison.OrdinalIgnoreCase))
        {
            return PinRole.Parent;
        }

        if (string.Equals(wishlist.ChildPin, pin, StringComparison.OrdinalIgnoreCase))
        {
            return PinRole.Child;
        }

        return PinRole.None;
    }

    private enum PinRole
    {
        None = 0,
        Parent = 1,
        Child = 2
    }
}

public record VerifyPinRequestDto(string Pin);

public record VerifyPinResponseDto(string Role);

public record AuthRequestDto(string Pin);

public record AddItemRequestDto(string Pin, string ItemName, string Category);

public record AddItemResponseDto(int Id, string ItemName, string Category, bool Bought);

public record WishlistItemDto(int Id, string ItemName, string Category, bool Bought);

public record MarkAsBoughtRequestDto(string Pin, bool Bought = true);
