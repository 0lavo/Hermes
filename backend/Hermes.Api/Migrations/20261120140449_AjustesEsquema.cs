using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hermes.Api.Migrations
{
    /// <inheritdoc />
    public partial class AjustesEsquema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_QuestionId_ParticipantId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_RoomId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Sessions");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Rooms",
                type: "char(6)",
                unicode: false,
                fixedLength: true,
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                columns: new[] { "QuestionId", "ParticipantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RoomId",
                table: "Sessions",
                column: "RoomId",
                unique: true,
                filter: "[EndedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_RoomId",
                table: "Sessions");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Votes",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Sessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Rooms",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(6)",
                oldUnicode: false,
                oldFixedLength: true,
                oldMaxLength: 6);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_QuestionId_ParticipantId",
                table: "Votes",
                columns: new[] { "QuestionId", "ParticipantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RoomId",
                table: "Sessions",
                column: "RoomId",
                unique: true,
                filter: "[Status] = 'Active'");
        }
    }
}
