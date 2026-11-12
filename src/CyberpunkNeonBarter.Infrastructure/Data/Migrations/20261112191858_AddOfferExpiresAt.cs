using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberpunkNeonBarter.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Offers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Offers");
        }
    }
}
