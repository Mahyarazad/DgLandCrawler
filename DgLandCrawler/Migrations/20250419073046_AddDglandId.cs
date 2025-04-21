using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DgLandCrawler.Migrations
{
    /// <inheritdoc />
    public partial class AddDglandId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DgLandId",
                table: "DGProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DgLandId",
                table: "DGProducts");
        }
    }
}
