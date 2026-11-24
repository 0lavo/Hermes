using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hermes.Api.Migrations
{
    /// <inheritdoc />
    public partial class AnonimatoPorDuvida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "Questions");
        }
    }
}
