using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class FixedIDinValorantAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                "PK_ValorantAccount",
                "ValorantAccount");

            migrationBuilder.AlterColumn<string>(
                "Subject",
                "ValorantAccount",
                "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                    "ValorantAccountId",
                    "ValorantAccount",
                    "INTEGER",
                    nullable: false,
                    defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                "PK_ValorantAccount",
                "ValorantAccount",
                "ValorantAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                "PK_ValorantAccount",
                "ValorantAccount");

            migrationBuilder.DropColumn(
                "ValorantAccountId",
                "ValorantAccount");

            migrationBuilder.AlterColumn<string>(
                "Subject",
                "ValorantAccount",
                "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                "PK_ValorantAccount",
                "ValorantAccount",
                "Subject");
        }
    }
}