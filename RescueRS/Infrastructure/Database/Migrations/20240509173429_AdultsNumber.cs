using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResgateRS.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdultsNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalPeopleNumber",
                table: "Rescues",
                newName: "AdultsNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdultsNumber",
                table: "Rescues",
                newName: "TotalPeopleNumber");
        }
    }
}
