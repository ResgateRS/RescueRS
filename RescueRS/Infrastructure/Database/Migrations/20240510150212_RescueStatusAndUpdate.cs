using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResgateRS.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RescueStatusAndUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rescued",
                table: "Rescues",
                newName: "Status");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdateDateTime",
                table: "Rescues",
                type: "TIMESTAMP(7) WITH TIME ZONE",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                table: "Rescues");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Rescues",
                newName: "Rescued");
        }
    }
}
