using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        // Get all unique categories
        app.MapGet("/categories", async (ApplicationDataContext db) =>
            {
                var categories = await db.Products
                    .Where(p => p.Category != null)
                    .Select(p => p.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return categories;
            })
            .Produces<List<string>>(StatusCodes.Status200OK)
            .WithDescription("Gets all unique product categories.");

        // Get all products with optional filters
        app.MapGet("/products", async (ApplicationDataContext db, string? category, decimal? maxUnitPrice) =>
            {
                var query = db.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(p => p.Category == category);
                }

                if (maxUnitPrice.HasValue)
                {
                    query = query.Where(p => p.PricePerUnit <= maxUnitPrice.Value);
                }

                var products = await query.Select(p => new ProductDto(
                    p.Id,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductDescription,
                    p.Category,
                    p.PricePerUnit
                )).ToListAsync();

                return products;
            })
            .Produces<List<ProductDto>>(StatusCodes.Status200OK)
            .WithDescription("Gets all products with optional filters for category and maxUnitPrice.");

        // Get a single product by ID
        app.MapGet("/products/{id}", async (int id, ApplicationDataContext db) =>
            {
                var product = await db.Products.FindAsync(id);

                if (product == null)
                {
                    return Results.NotFound();
                }

                var result = new ProductDto(
                    product.Id,
                    product.ProductCode,
                    product.ProductName,
                    product.ProductDescription,
                    product.Category,
                    product.PricePerUnit
                );

                return Results.Ok(result);
            })
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Gets a single product by ID.");

        // Update a product
        app.MapPut("/products/{id}", async (int id, ProductUpdateDto dto, ApplicationDataContext db) =>
            {
                var product = await db.Products.FindAsync(id);
                if (product == null)
                {
                    return Results.NotFound("Product not found.");
                }

                // Validate product code is not empty and length <= 10
                if (string.IsNullOrWhiteSpace(dto.ProductCode))
                {
                    return Results.BadRequest("Product code must not be empty.");
                }
                if (dto.ProductCode.Length > 10)
                {
                    return Results.BadRequest("Product code must not exceed 10 characters.");
                }

                // Validate product name is not empty and length <= 100
                if (string.IsNullOrWhiteSpace(dto.ProductName))
                {
                    return Results.BadRequest("Product name must not be empty.");
                }
                if (dto.ProductName.Length > 100)
                {
                    return Results.BadRequest("Product name must not exceed 100 characters.");
                }

                // Validate product description length <= 255 (if provided)
                if (dto.ProductDescription != null && dto.ProductDescription.Length > 255)
                {
                    return Results.BadRequest("Product description must not exceed 255 characters.");
                }

                // Validate category length <= 50 (if provided)
                if (dto.Category != null && dto.Category.Length > 50)
                {
                    return Results.BadRequest("Category must not exceed 50 characters.");
                }

                // Validate price per unit is positive
                if (dto.PricePerUnit <= 0)
                {
                    return Results.BadRequest("Price per unit must be greater than zero.");
                }

                product.ProductCode = dto.ProductCode;
                product.ProductName = dto.ProductName;
                product.ProductDescription = dto.ProductDescription;
                product.Category = dto.Category;
                product.PricePerUnit = dto.PricePerUnit;

                await db.SaveChangesAsync();

                var result = new ProductDto(
                    product.Id,
                    product.ProductCode,
                    product.ProductName,
                    product.ProductDescription,
                    product.Category,
                    product.PricePerUnit
                );

                return Results.Ok(result);
            })
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Updates an existing product.");

        // Delete a product
        app.MapDelete("/products/{id}", async (int id, ApplicationDataContext db) =>
            {
                var product = await db.Products.FindAsync(id);
                if (product == null)
                {
                    return Results.NotFound();
                }

                db.Products.Remove(product);
                await db.SaveChangesAsync();
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithDescription("Deletes a product.");

        return app;
    }
}

public record ProductDto(
    int Id,
    string ProductCode,
    string ProductName,
    string? ProductDescription,
    string? Category,
    decimal PricePerUnit);

public record ProductUpdateDto(
    string ProductCode,
    string ProductName,
    string? ProductDescription,
    string? Category,
    decimal PricePerUnit);
