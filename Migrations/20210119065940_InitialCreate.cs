using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "DiscordUsers",
                table => new
                {
                    DiscordUserId = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>("TEXT", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_DiscordUsers", x => x.DiscordUserId); });

            migrationBuilder.CreateTable(
                "ValorantAccount",
                table => new
                {
                    ValorantAccountId = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject = table.Column<string>("TEXT", nullable: true),
                    DisplayName = table.Column<string>("TEXT", nullable: true),
                    Rank = table.Column<int>("INTEGER", nullable: false),
                    RankName = table.Column<string>("TEXT", nullable: true),
                    RankProgress = table.Column<int>("INTEGER", nullable: false),
                    DiscordUserId = table.Column<int>("INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValorantAccount", x => x.ValorantAccountId);
                    table.ForeignKey(
                        "FK_ValorantAccount_DiscordUsers_DiscordUserId",
                        x => x.DiscordUserId,
                        "DiscordUsers",
                        "DiscordUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_ValorantAccount_DiscordUserId",
                "ValorantAccount",
                "DiscordUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "ValorantAccount");

            migrationBuilder.DropTable(
                "DiscordUsers");
        }
    }
}