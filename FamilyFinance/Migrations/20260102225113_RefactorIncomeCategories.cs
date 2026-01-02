using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinance.Migrations
{
    /// <inheritdoc />
    public partial class RefactorIncomeCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "MonthlyIncomes",
                newName: "CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "BudgetCategories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // === SEEDING & DATA FIX ===
            migrationBuilder.Sql(@"
                INSERT INTO BudgetCategories (FamilyId, Name, Icon, Color, MonthlyBudget, SortOrder, IsActive, CreatedAt, IsDeleted, Type)
                SELECT Id, 'Stipendio', '💰', '#10b981', 0, 100, 1, datetime('now'), 0, 1 FROM Families;
                
                INSERT INTO BudgetCategories (FamilyId, Name, Icon, Color, MonthlyBudget, SortOrder, IsActive, CreatedAt, IsDeleted, Type)
                SELECT Id, 'Regali', '🎁', '#f472b6', 0, 101, 1, datetime('now'), 0, 1 FROM Families;
                
                INSERT INTO BudgetCategories (FamilyId, Name, Icon, Color, MonthlyBudget, SortOrder, IsActive, CreatedAt, IsDeleted, Type)
                SELECT Id, 'Rimborsi', '↩️', '#60a5fa', 0, 102, 1, datetime('now'), 0, 1 FROM Families;

                INSERT INTO BudgetCategories (FamilyId, Name, Icon, Color, MonthlyBudget, SortOrder, IsActive, CreatedAt, IsDeleted, Type)
                SELECT Id, 'Investimenti', '📈', '#34d399', 0, 103, 1, datetime('now'), 0, 1 FROM Families;
                
                INSERT INTO BudgetCategories (FamilyId, Name, Icon, Color, MonthlyBudget, SortOrder, IsActive, CreatedAt, IsDeleted, Type)
                SELECT Id, 'Altro', '📦', '#9ca3af', 0, 104, 1, datetime('now'), 0, 1 FROM Families;
            ");

            migrationBuilder.Sql(@"
                UPDATE MonthlyIncomes SET CategoryId = (SELECT Id FROM BudgetCategories WHERE Name='Stipendio' AND FamilyId=(SELECT FamilyId FROM Snapshots WHERE Id=MonthlyIncomes.SnapshotId) AND Type=1) WHERE CategoryId = 0;
                UPDATE MonthlyIncomes SET CategoryId = (SELECT Id FROM BudgetCategories WHERE Name='Regali' AND FamilyId=(SELECT FamilyId FROM Snapshots WHERE Id=MonthlyIncomes.SnapshotId) AND Type=1) WHERE CategoryId = 1;
                UPDATE MonthlyIncomes SET CategoryId = (SELECT Id FROM BudgetCategories WHERE Name='Rimborsi' AND FamilyId=(SELECT FamilyId FROM Snapshots WHERE Id=MonthlyIncomes.SnapshotId) AND Type=1) WHERE CategoryId = 2;
                UPDATE MonthlyIncomes SET CategoryId = (SELECT Id FROM BudgetCategories WHERE Name='Investimenti' AND FamilyId=(SELECT FamilyId FROM Snapshots WHERE Id=MonthlyIncomes.SnapshotId) AND Type=1) WHERE CategoryId = 3;
                UPDATE MonthlyIncomes SET CategoryId = (SELECT Id FROM BudgetCategories WHERE Name='Altro' AND FamilyId=(SELECT FamilyId FROM Snapshots WHERE Id=MonthlyIncomes.SnapshotId) AND Type=1) WHERE CategoryId = 4;
            ");
            // ==========================

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyIncomes_CategoryId",
                table: "MonthlyIncomes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyIncomes_BudgetCategories_CategoryId",
                table: "MonthlyIncomes",
                column: "CategoryId",
                principalTable: "BudgetCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyIncomes_BudgetCategories_CategoryId",
                table: "MonthlyIncomes");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyIncomes_CategoryId",
                table: "MonthlyIncomes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "BudgetCategories");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "MonthlyIncomes",
                newName: "Category");
        }
    }
}
