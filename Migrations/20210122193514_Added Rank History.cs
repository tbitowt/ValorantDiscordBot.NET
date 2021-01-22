using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class AddedRankHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RankInfo",
                columns: table => new
                {
                    RankInfoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ValorantAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    RankInt = table.Column<int>(type: "INTEGER", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankInfo", x => x.RankInfoId);
                    table.ForeignKey(
                        name: "FK_RankInfo_ValorantAccount_ValorantAccountId",
                        column: x => x.ValorantAccountId,
                        principalTable: "ValorantAccount",
                        principalColumn: "ValorantAccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RankInfo_ValorantAccountId",
                table: "RankInfo",
                column: "ValorantAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RankInfo");
        }
    }
}
