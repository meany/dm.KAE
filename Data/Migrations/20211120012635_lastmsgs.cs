using Microsoft.EntityFrameworkCore.Migrations;

namespace dm.KAE.Data.Migrations
{
    public partial class lastmsgs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LastMessages",
                columns: table => new
                {
                    LastMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastMessages", x => x.LastMessageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LastMessages_ChatId",
                table: "LastMessages",
                column: "ChatId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LastMessages");
        }
    }
}
