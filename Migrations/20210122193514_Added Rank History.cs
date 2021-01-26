using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class AddedRankHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "RankInfo",
                table => new
                {
                    RankInfoId = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ValorantAccountId = table.Column<int>("INTEGER", nullable: true),
                    RankInt = table.Column<int>("INTEGER", nullable: false),
                    Progress = table.Column<int>("INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>("TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankInfo", x => x.RankInfoId);
                    table.ForeignKey(
                        "FK_RankInfo_ValorantAccount_ValorantAccountId",
                        x => x.ValorantAccountId,
                        "ValorantAccount",
                        "ValorantAccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_RankInfo_ValorantAccountId",
                "RankInfo",
                "ValorantAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "RankInfo");
        }
    }
}