using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace praesentationsanmeldung.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Benutzername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswortHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "G3Sus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vorname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nachname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Klasse = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_G3Sus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Raeume",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bezeichnung = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kapazitaet = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Raeume", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Praesentationen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Beginn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RaumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Praesentationen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Praesentationen_Raeume_RaumId",
                        column: x => x.RaumId,
                        principalTable: "Raeume",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Eintragungen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EingetragenAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    G3SusId = table.Column<int>(type: "int", nullable: false),
                    PraesentationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eintragungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Eintragungen_G3Sus_G3SusId",
                        column: x => x.G3SusId,
                        principalTable: "G3Sus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Eintragungen_Praesentationen_PraesentationId",
                        column: x => x.PraesentationId,
                        principalTable: "Praesentationen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Benutzername",
                table: "Admins",
                column: "Benutzername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eintragungen_G3SusId_PraesentationId",
                table: "Eintragungen",
                columns: new[] { "G3SusId", "PraesentationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Eintragungen_PraesentationId",
                table: "Eintragungen",
                column: "PraesentationId");

            migrationBuilder.CreateIndex(
                name: "IX_Praesentationen_RaumId",
                table: "Praesentationen",
                column: "RaumId");

            migrationBuilder.CreateIndex(
                name: "IX_Raeume_Bezeichnung",
                table: "Raeume",
                column: "Bezeichnung",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Eintragungen");

            migrationBuilder.DropTable(
                name: "G3Sus");

            migrationBuilder.DropTable(
                name: "Praesentationen");

            migrationBuilder.DropTable(
                name: "Raeume");
        }
    }
}
