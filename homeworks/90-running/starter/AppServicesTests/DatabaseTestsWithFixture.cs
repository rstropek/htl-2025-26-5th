using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;
using Xunit;

namespace AppServicesTests;

public class DatabaseTestsWithFixture(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task HasSeededLaufkategorien()
    {
        var kategorien = await fixture.Context.Laufkategorien.ToListAsync();
        Assert.Equal(4, kategorien.Count);
        Assert.Contains(kategorien, k => k.Bezeichnung == "Straßenlauf");
        Assert.Contains(kategorien, k => k.Bezeichnung == "Crosslauf");
        Assert.Contains(kategorien, k => k.Bezeichnung == "Bahnlauf");
        Assert.Contains(kategorien, k => k.Bezeichnung == "Trailrun");
    }

    [Fact]
    public async Task CanInsertSelectDeleteLaufbewerb()
    {
        var bewerb = new Laufbewerb
        {
            Name = "Test Lauf",
            LaufkategorieId = 1,
            Streckenlänge = 10.0m,
            Datum = new DateOnly(2026, 5, 1),
            Ort = "Wien"
        };
        fixture.Context.Laufbewerbe.Add(bewerb);
        await fixture.Context.SaveChangesAsync();

        var loaded = await fixture.Context.Laufbewerbe.FirstAsync(b => b.Id == bewerb.Id);
        Assert.Equal("Test Lauf", loaded.Name);
        Assert.Equal("Wien", loaded.Ort);

        fixture.Context.Laufbewerbe.Remove(loaded);
        await fixture.Context.SaveChangesAsync();

        var deleted = await fixture.Context.Laufbewerbe.FindAsync(bewerb.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task CanInsertSelectDeleteTeilnehmer()
    {
        var bewerb = new Laufbewerb
        {
            Name = "Test Lauf 2",
            LaufkategorieId = 1,
            Streckenlänge = 5.0m,
            Datum = new DateOnly(2026, 6, 1),
            Ort = "Graz"
        };
        fixture.Context.Laufbewerbe.Add(bewerb);
        await fixture.Context.SaveChangesAsync();

        var teilnehmer = new Teilnehmer
        {
            LaufbewerbId = bewerb.Id,
            Startnummer = 42,
            Vorname = "Max",
            Nachname = "Muster",
            AngestrebteGesamtzeit = 1800,
        };
        fixture.Context.Teilnehmer.Add(teilnehmer);
        await fixture.Context.SaveChangesAsync();

        var loaded = await fixture.Context.Teilnehmer.FirstAsync(t => t.Id == teilnehmer.Id);
        Assert.Equal("Max", loaded.Vorname);

        fixture.Context.Teilnehmer.Remove(loaded);
        await fixture.Context.SaveChangesAsync();

        var deleted = await fixture.Context.Teilnehmer.FindAsync(teilnehmer.Id);
        Assert.Null(deleted);

        fixture.Context.Laufbewerbe.Remove(bewerb);
        await fixture.Context.SaveChangesAsync();
    }

    [Fact]
    public async Task CanInsertSelectDeleteSplit()
    {
        var bewerb = new Laufbewerb
        {
            Name = "Test Lauf 3",
            LaufkategorieId = 1,
            Streckenlänge = 3.0m,
            Datum = new DateOnly(2026, 7, 1),
            Ort = "Linz"
        };
        fixture.Context.Laufbewerbe.Add(bewerb);
        await fixture.Context.SaveChangesAsync();

        var teilnehmer = new Teilnehmer
        {
            LaufbewerbId = bewerb.Id,
            Startnummer = 1,
            Vorname = "Anna",
            Nachname = "Bauer",
            AngestrebteGesamtzeit = 900,
        };
        fixture.Context.Teilnehmer.Add(teilnehmer);
        await fixture.Context.SaveChangesAsync();

        var split = new Split
        {
            TeilnehmerId = teilnehmer.Id,
            KmNummer = 1,
            ZeitSekunden = 300,
            SegmentLaenge = 1.0m,
        };
        fixture.Context.Splits.Add(split);
        await fixture.Context.SaveChangesAsync();

        var loaded = await fixture.Context.Splits.FirstAsync(s => s.Id == split.Id);
        Assert.Equal(1, loaded.KmNummer);
        Assert.Equal(300, loaded.ZeitSekunden);

        fixture.Context.Splits.Remove(loaded);
        await fixture.Context.SaveChangesAsync();

        fixture.Context.Teilnehmer.Remove(teilnehmer);
        fixture.Context.Laufbewerbe.Remove(bewerb);
        await fixture.Context.SaveChangesAsync();
    }
}
