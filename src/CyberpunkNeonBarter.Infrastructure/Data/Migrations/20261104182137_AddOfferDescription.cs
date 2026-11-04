using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberpunkNeonBarter.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Offers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Offers");
        }
    }
}
