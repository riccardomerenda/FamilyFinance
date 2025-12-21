using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortfolioId",
                table: "InvestmentAssets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TimeHorizonYears = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetYear = table.Column<int>(type: "INTEGER", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentAssets_PortfolioId",
                table: "InvestmentAssets",
                column: "PortfolioId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvestmentAssets_Portfolios_PortfolioId",
                table: "InvestmentAssets",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvestmentAssets_Portfolios_PortfolioId",
                table: "InvestmentAssets");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropIndex(
                name: "IX_InvestmentAssets_PortfolioId",
                table: "InvestmentAssets");

            migrationBuilder.DropColumn(
                name: "PortfolioId",
                table: "InvestmentAssets");
        }
    }
}
