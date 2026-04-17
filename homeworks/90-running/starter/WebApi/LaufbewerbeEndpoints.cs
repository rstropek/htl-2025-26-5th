using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class LaufbewerbeEndpoints
{
    public static IEndpointRouteBuilder MapLaufbewerbeEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: Add endpoints for Laufbewerbe management:
        //   GET  /laufkategorien       - List all Laufkategorien
        //   GET  /laufbewerbe          - List all Laufbewerbe (filtered by name, laufkategorieId; sorted by Datum DESC)
        //   GET  /laufbewerbe/{id}     - Get a Laufbewerb by id
        //   POST /laufbewerbe          - Create a new Laufbewerb (with validation)
        //   PATCH /laufbewerbe/{id}    - Partially update a Laufbewerb (with validation)
        //   DELETE /laufbewerbe/{id}   - Delete a Laufbewerb

        return app;
    }
}

// TODO: Add record types for DTOs here (LaufkategorieDto, LaufbewerbDto, CreateOrUpdateLaufbewerbDto, PatchLaufbewerbDto)
