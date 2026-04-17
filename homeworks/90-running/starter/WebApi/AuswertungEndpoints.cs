using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class AuswertungEndpoints
{
    public static IEndpointRouteBuilder MapAuswertungEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: Add endpoints for Auswertung:
        //   GET  /laufbewerbe/{id}/teilnehmer  - List all Teilnehmer for a Laufbewerb (sorted by Startnummer)
        //   POST /laufbewerbe/auswertung       - Compute split evaluation for a Teilnehmer

        return app;
    }
}

// TODO: Add record types for DTOs here (TeilnehmerDto, AuswertungRequest, AuswertungRowDto, AuswertungTotalsDto, AuswertungResponse)
