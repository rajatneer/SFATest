using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SfaApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class MobileTimezoneAndMultiSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckinTimeZoneId",
                table: "Visits",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CheckinUtcOffsetMinutes",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutTimeZoneId",
                table: "Visits",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CheckoutUtcOffsetMinutes",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "SalesOrders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "UtcOffsetMinutes",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndDayTimeZoneId",
                table: "DaySessions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EndDayUtcOffsetMinutes",
                table: "DaySessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartDayTimeZoneId",
                table: "DaySessions",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "StartDayUtcOffsetMinutes",
                table: "DaySessions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckinTimeZoneId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckinUtcOffsetMinutes",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckoutTimeZoneId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckoutUtcOffsetMinutes",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "UtcOffsetMinutes",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "EndDayTimeZoneId",
                table: "DaySessions");

            migrationBuilder.DropColumn(
                name: "EndDayUtcOffsetMinutes",
                table: "DaySessions");

            migrationBuilder.DropColumn(
                name: "StartDayTimeZoneId",
                table: "DaySessions");

            migrationBuilder.DropColumn(
                name: "StartDayUtcOffsetMinutes",
                table: "DaySessions");
        }
    }
}
