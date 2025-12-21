using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddShowMonthlyTargetToGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowMonthlyTarget",
                table: "Goals",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowMonthlyTarget",
                table: "Goals");
        }
    }
}
