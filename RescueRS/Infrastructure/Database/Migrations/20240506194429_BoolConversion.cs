using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResgateRS.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class BoolConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Rescuer",
                table: "Users",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "NUMBER(3)");

            migrationBuilder.AlterColumn<int>(
                name: "Rescued",
                table: "Rescues",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "NUMBER(3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Rescuer",
                table: "Users",
                type: "NUMBER(3)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<byte>(
                name: "Rescued",
                table: "Rescues",
                type: "NUMBER(3)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");
        }
    }
}
