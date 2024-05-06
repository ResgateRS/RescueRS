using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResgateRS.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CellphoneTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Celphone",
                table: "Users",
                newName: "Cellphone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cellphone",
                table: "Users",
                newName: "Celphone");
        }
    }
}
