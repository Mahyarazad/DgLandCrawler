using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DgLandCrawler.Migrations
{
    /// <inheritdoc />
    public partial class CreationTimeGoogle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "GoogleSearchResults",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "GoogleSearchResults");
        }
    }
}
