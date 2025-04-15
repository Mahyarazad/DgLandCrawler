using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DgLandCrawler.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DGProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegularPrice = table.Column<int>(type: "int", nullable: false),
                    CrawlDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_DGProductId", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoogleSearchResults",
                columns: table => new
                {
                    GoogleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreviousPrice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DGProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_GoogleId", x => x.GoogleId);
                    table.ForeignKey(
                        name: "FK_GoogleSearchResults_DGProducts_DGProductId",
                        column: x => x.DGProductId,
                        principalTable: "DGProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleSearchResults_DGProductId",
                table: "GoogleSearchResults",
                column: "DGProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleSearchResults");

            migrationBuilder.DropTable(
                name: "DGProducts");
        }
    }
}
