using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class Fixedguildconnections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                "UpdatesChannel",
                "GuildConfigs",
                "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                "RegisteredGuild",
                table => new
                {
                    RegisteredGuildID = table.Column<int>("INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ValorantAccountId = table.Column<int>("INTEGER", nullable: true),
                    GuildID = table.Column<ulong>("INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredGuild", x => x.RegisteredGuildID);
                    table.ForeignKey(
                        "FK_RegisteredGuild_ValorantAccount_ValorantAccountId",
                        x => x.ValorantAccountId,
                        "ValorantAccount",
                        "ValorantAccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_RegisteredGuild_ValorantAccountId",
                "RegisteredGuild",
                "ValorantAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "RegisteredGuild");

            migrationBuilder.AlterColumn<ulong>(
                "UpdatesChannel",
                "GuildConfigs",
                "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}