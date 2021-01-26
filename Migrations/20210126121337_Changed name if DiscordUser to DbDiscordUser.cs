using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class ChangednameifDiscordUsertoDbDiscordUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_ValorantAccount_DiscordUsers_DiscordUserId",
                "ValorantAccount");

            migrationBuilder.RenameColumn(
                "DiscordUserId",
                "ValorantAccount",
                "DbDiscordUserID");

            migrationBuilder.RenameIndex(
                "IX_ValorantAccount_DiscordUserId",
                table: "ValorantAccount",
                newName: "IX_ValorantAccount_DbDiscordUserID");

            migrationBuilder.RenameColumn(
                "DiscordUserId",
                "DiscordUsers",
                "ID");

            migrationBuilder.AddForeignKey(
                "FK_ValorantAccount_DiscordUsers_DbDiscordUserID",
                "ValorantAccount",
                "DbDiscordUserID",
                "DiscordUsers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_ValorantAccount_DiscordUsers_DbDiscordUserID",
                "ValorantAccount");

            migrationBuilder.RenameColumn(
                "DbDiscordUserID",
                "ValorantAccount",
                "DiscordUserId");

            migrationBuilder.RenameIndex(
                "IX_ValorantAccount_DbDiscordUserID",
                table: "ValorantAccount",
                newName: "IX_ValorantAccount_DiscordUserId");

            migrationBuilder.RenameColumn(
                "ID",
                "DiscordUsers",
                "DiscordUserId");

            migrationBuilder.AddForeignKey(
                "FK_ValorantAccount_DiscordUsers_DiscordUserId",
                "ValorantAccount",
                "DiscordUserId",
                "DiscordUsers",
                principalColumn: "DiscordUserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}