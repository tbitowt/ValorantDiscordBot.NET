using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class FixedIDinValorantAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ValorantAccount",
                table: "ValorantAccount");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "ValorantAccount",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ValorantAccountId",
                table: "ValorantAccount",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ValorantAccount",
                table: "ValorantAccount",
                column: "ValorantAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ValorantAccount",
                table: "ValorantAccount");

            migrationBuilder.DropColumn(
                name: "ValorantAccountId",
                table: "ValorantAccount");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "ValorantAccount",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ValorantAccount",
                table: "ValorantAccount",
                column: "Subject");
        }
    }
}
