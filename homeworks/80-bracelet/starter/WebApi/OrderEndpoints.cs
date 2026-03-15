using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

// TODO: Add at least two meaningful integration tests for the order endpoints in WebApiTests/OrderIntegrationTests.cs

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: Add endpoints for bracelet orders

        return app;
    }
}

// TODO: Add record type for DTOs here