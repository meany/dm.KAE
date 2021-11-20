using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace dm.KAE.Data.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holders",
                columns: table => new
                {
                    HolderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastBlockNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holders", x => x.HolderId);
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    PriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Group = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceUSD = table.Column<decimal>(type: "decimal(11,6)", nullable: false),
                    PriceUSDChange = table.Column<int>(type: "int", nullable: false),
                    PriceUSDChangePct = table.Column<decimal>(type: "decimal(12,8)", nullable: false),
                    PriceETH = table.Column<decimal>(type: "decimal(16,8)", nullable: false),
                    PriceETHChange = table.Column<int>(type: "int", nullable: false),
                    PriceETHChangePct = table.Column<decimal>(type: "decimal(12,8)", nullable: false),
                    PriceBTC = table.Column<decimal>(type: "decimal(16,8)", nullable: false),
                    PriceBTCChange = table.Column<int>(type: "int", nullable: false),
                    PriceBTCChangePct = table.Column<decimal>(type: "decimal(12,8)", nullable: false),
                    MarketCapUSD = table.Column<int>(type: "int", nullable: false),
                    MarketCapUSDChange = table.Column<int>(type: "int", nullable: false),
                    MarketCapUSDChangePct = table.Column<decimal>(type: "decimal(12,8)", nullable: false),
                    VolumeUSD = table.Column<int>(type: "int", nullable: false),
                    VolumeUSDChange = table.Column<int>(type: "int", nullable: false),
                    VolumeUSDChangePct = table.Column<decimal>(type: "decimal(12,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.PriceId);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    User = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Response = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "Stats",
                columns: table => new
                {
                    StatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Group = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Transactions = table.Column<int>(type: "int", nullable: false),
                    Supply = table.Column<decimal>(type: "decimal(25,18)", nullable: false),
                    Circulation = table.Column<decimal>(type: "decimal(25,18)", nullable: false),
                    Holders = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => x.StatId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holders_Value",
                table: "Holders",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_Date",
                table: "Prices",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_Group",
                table: "Prices",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Date",
                table: "Requests",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Response_Type",
                table: "Requests",
                columns: new[] { "Response", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Stats_Date",
                table: "Stats",
                column: "Date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holders");

            migrationBuilder.DropTable(
                name: "Prices");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Stats");
        }
    }
}
