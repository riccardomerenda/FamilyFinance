using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringInvestmentTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetAssetHoldingId",
                table: "RecurringTransactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetPensionAccountId",
                table: "RecurringTransactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_TargetAssetHoldingId",
                table: "RecurringTransactions",
                column: "TargetAssetHoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_TargetPensionAccountId",
                table: "RecurringTransactions",
                column: "TargetPensionAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransactions_Accounts_TargetPensionAccountId",
                table: "RecurringTransactions",
                column: "TargetPensionAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransactions_AssetHoldings_TargetAssetHoldingId",
                table: "RecurringTransactions",
                column: "TargetAssetHoldingId",
                principalTable: "AssetHoldings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_Accounts_TargetPensionAccountId",
                table: "RecurringTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_AssetHoldings_TargetAssetHoldingId",
                table: "RecurringTransactions");

            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactions_TargetAssetHoldingId",
                table: "RecurringTransactions");

            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactions_TargetPensionAccountId",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "TargetAssetHoldingId",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "TargetPensionAccountId",
                table: "RecurringTransactions");
        }
    }
}
