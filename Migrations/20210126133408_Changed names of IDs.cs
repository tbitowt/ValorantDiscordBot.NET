using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class ChangednamesofIDs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_RankInfo_ValorantAccount_ValorantAccountId",
                "RankInfo");

            migrationBuilder.DropForeignKey(
                "FK_RegisteredGuild_ValorantAccount_ValorantAccountId",
                "RegisteredGuild");

            migrationBuilder.RenameColumn(
                "ValorantAccountId",
                "ValorantAccount",
                "ID");

            migrationBuilder.RenameColumn(
                "ValorantAccountId",
                "RegisteredGuild",
                "ValorantAccountID");

            migrationBuilder.RenameColumn(
                "RegisteredGuildID",
                "RegisteredGuild",
                "ID");

            migrationBuilder.RenameIndex(
                "IX_RegisteredGuild_ValorantAccountId",
                table: "RegisteredGuild",
                newName: "IX_RegisteredGuild_ValorantAccountID");

            migrationBuilder.RenameColumn(
                "ValorantAccountId",
                "RankInfo",
                "ValorantAccountID");

            migrationBuilder.RenameColumn(
                "RankInfoId",
                "RankInfo",
                "ID");

            migrationBuilder.RenameIndex(
                "IX_RankInfo_ValorantAccountId",
                table: "RankInfo",
                newName: "IX_RankInfo_ValorantAccountID");

            migrationBuilder.AddForeignKey(
                "FK_RankInfo_ValorantAccount_ValorantAccountID",
                "RankInfo",
                "ValorantAccountID",
                "ValorantAccount",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                "FK_RegisteredGuild_ValorantAccount_ValorantAccountID",
                "RegisteredGuild",
                "ValorantAccountID",
                "ValorantAccount",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_RankInfo_ValorantAccount_ValorantAccountID",
                "RankInfo");

            migrationBuilder.DropForeignKey(
                "FK_RegisteredGuild_ValorantAccount_ValorantAccountID",
                "RegisteredGuild");

            migrationBuilder.RenameColumn(
                "ID",
                "ValorantAccount",
                "ValorantAccountId");

            migrationBuilder.RenameColumn(
                "ValorantAccountID",
                "RegisteredGuild",
                "ValorantAccountId");

            migrationBuilder.RenameColumn(
                "ID",
                "RegisteredGuild",
                "RegisteredGuildID");

            migrationBuilder.RenameIndex(
                "IX_RegisteredGuild_ValorantAccountID",
                table: "RegisteredGuild",
                newName: "IX_RegisteredGuild_ValorantAccountId");

            migrationBuilder.RenameColumn(
                "ValorantAccountID",
                "RankInfo",
                "ValorantAccountId");

            migrationBuilder.RenameColumn(
                "ID",
                "RankInfo",
                "RankInfoId");

            migrationBuilder.RenameIndex(
                "IX_RankInfo_ValorantAccountID",
                table: "RankInfo",
                newName: "IX_RankInfo_ValorantAccountId");

            migrationBuilder.AddForeignKey(
                "FK_RankInfo_ValorantAccount_ValorantAccountId",
                "RankInfo",
                "ValorantAccountId",
                "ValorantAccount",
                principalColumn: "ValorantAccountId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                "FK_RegisteredGuild_ValorantAccount_ValorantAccountId",
                "RegisteredGuild",
                "ValorantAccountId",
                "ValorantAccount",
                principalColumn: "ValorantAccountId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}