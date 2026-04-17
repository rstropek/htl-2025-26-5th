using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AppServices.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Laufkategorien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bezeichnung = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laufkategorien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Laufbewerbe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LaufkategorieId = table.Column<int>(type: "INTEGER", nullable: false),
                    Streckenlänge = table.Column<double>(type: "REAL", nullable: false),
                    Datum = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Ort = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laufbewerbe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Laufbewerbe_Laufkategorien_LaufkategorieId",
                        column: x => x.LaufkategorieId,
                        principalTable: "Laufkategorien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Teilnehmer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LaufbewerbId = table.Column<int>(type: "INTEGER", nullable: false),
                    Startnummer = table.Column<int>(type: "INTEGER", nullable: false),
                    Vorname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nachname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AngestrebteGesamtzeit = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teilnehmer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teilnehmer_Laufbewerbe_LaufbewerbId",
                        column: x => x.LaufbewerbId,
                        principalTable: "Laufbewerbe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Splits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeilnehmerId = table.Column<int>(type: "INTEGER", nullable: false),
                    KmNummer = table.Column<int>(type: "INTEGER", nullable: false),
                    ZeitSekunden = table.Column<int>(type: "INTEGER", nullable: false),
                    SegmentLaenge = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Splits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Splits_Teilnehmer_TeilnehmerId",
                        column: x => x.TeilnehmerId,
                        principalTable: "Teilnehmer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Laufkategorien",
                columns: new[] { "Id", "Bezeichnung" },
                values: new object[,]
                {
                    { 1, "Straßenlauf" },
                    { 2, "Crosslauf" },
                    { 3, "Bahnlauf" },
                    { 4, "Trailrun" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Laufbewerbe_LaufkategorieId",
                table: "Laufbewerbe",
                column: "LaufkategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Splits_TeilnehmerId",
                table: "Splits",
                column: "TeilnehmerId");

            migrationBuilder.CreateIndex(
                name: "IX_Teilnehmer_LaufbewerbId",
                table: "Teilnehmer",
                column: "LaufbewerbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Splits");

            migrationBuilder.DropTable(
                name: "Teilnehmer");

            migrationBuilder.DropTable(
                name: "Laufbewerbe");

            migrationBuilder.DropTable(
                name: "Laufkategorien");
        }
    }
}
