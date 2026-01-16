using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinance.Migrations
{
    /// <inheritdoc />
    public partial class FixRecurringMatchRuleNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringMatchRules_RecurringTransactions_RecurringTransactionId",
                table: "RecurringMatchRules");

            migrationBuilder.AlterColumn<int>(
                name: "RecurringTransactionId",
                table: "RecurringMatchRules",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringMatchRules_RecurringTransactions_RecurringTransactionId",
                table: "RecurringMatchRules",
                column: "RecurringTransactionId",
                principalTable: "RecurringTransactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringMatchRules_RecurringTransactions_RecurringTransactionId",
                table: "RecurringMatchRules");

            migrationBuilder.AlterColumn<int>(
                name: "RecurringTransactionId",
                table: "RecurringMatchRules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurringMatchRules_RecurringTransactions_RecurringTransactionId",
                table: "RecurringMatchRules",
                column: "RecurringTransactionId",
                principalTable: "RecurringTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
