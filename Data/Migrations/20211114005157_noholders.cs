using Microsoft.EntityFrameworkCore.Migrations;

namespace dm.KAE.Data.Migrations
{
    public partial class noholders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Holders",
                table: "Stats");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Holders",
                table: "Stats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
