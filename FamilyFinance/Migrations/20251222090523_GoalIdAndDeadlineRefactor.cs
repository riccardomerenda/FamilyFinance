using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinance.Migrations
{
    /// <inheritdoc />
    public partial class GoalIdAndDeadlineRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Snapshots_FamilyId",
                table: "Snapshots");

            migrationBuilder.AlterColumn<string>(
                name: "Deadline",
                table: "Goals",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Snapshots_FamilyId_SnapshotDate",
                table: "Snapshots",
                columns: new[] { "FamilyId", "SnapshotDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Snapshots_FamilyId_SnapshotDate",
                table: "Snapshots");

            migrationBuilder.AlterColumn<string>(
                name: "Deadline",
                table: "Goals",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Snapshots_FamilyId",
                table: "Snapshots",
                column: "FamilyId");
        }
    }
}
