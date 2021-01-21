using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBot.Migrations
{
    public partial class Fixedguildconnections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ulong>(
                name: "UpdatesChannel",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "RegisteredGuild",
                columns: table => new
                {
                    RegisteredGuildID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ValorantAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    GuildID = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredGuild", x => x.RegisteredGuildID);
                    table.ForeignKey(
                        name: "FK_RegisteredGuild_ValorantAccount_ValorantAccountId",
                        column: x => x.ValorantAccountId,
                        principalTable: "ValorantAccount",
                        principalColumn: "ValorantAccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredGuild_ValorantAccountId",
                table: "RegisteredGuild",
                column: "ValorantAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegisteredGuild");

            migrationBuilder.AlterColumn<ulong>(
                name: "UpdatesChannel",
                table: "GuildConfigs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
