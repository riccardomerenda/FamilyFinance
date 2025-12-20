using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyFinance.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionBasisToSnapshotLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ContributionBasis",
                table: "SnapshotLines",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContributionBasis",
                table: "SnapshotLines");
        }
    }
}
