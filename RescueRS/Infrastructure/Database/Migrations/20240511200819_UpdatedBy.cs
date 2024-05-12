using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResgateRS.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Rescues",
                type: "RAW(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Rescues");
        }
    }
}
