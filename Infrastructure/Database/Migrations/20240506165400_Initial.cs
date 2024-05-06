using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResgateRS.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rescues",
                columns: table => new
                {
                    RescueId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RequestDateTime = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: false),
                    TotalPeopleNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ChildrenNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ElderlyNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DisabledNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AnimalsNumber = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Latitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Longitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Rescued = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    RescueDateTime = table.Column<DateTimeOffset>(type: "TIMESTAMP(7) WITH TIME ZONE", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ContactPhone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rescues", x => x.RescueId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Rescuer = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    Celphone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rescues");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
