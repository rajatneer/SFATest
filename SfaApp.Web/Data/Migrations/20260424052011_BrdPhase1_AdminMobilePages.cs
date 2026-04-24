using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SfaApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class BrdPhase1_AdminMobilePages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CheckinLat",
                table: "Visits",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckinLong",
                table: "Visits",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckinTimestampUtc",
                table: "Visits",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckoutLat",
                table: "Visits",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckoutLong",
                table: "Visits",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckoutTimestampUtc",
                table: "Visits",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientGeneratedUuid",
                table: "Visits",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "CoordinateCapturedDuringVisitFlag",
                table: "Visits",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CustomerRefLat",
                table: "Visits",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CustomerRefLong",
                table: "Visits",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaySessionId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GeoDistanceMeters",
                table: "Visits",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepUserId",
                table: "Visits",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SyncStatus",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VisitStatus",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WithinToleranceFlag",
                table: "Visits",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientGeneratedUuid",
                table: "SalesOrders",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DaySessionId",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistributorId",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossAmount",
                table: "SalesOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "SalesOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RepUserId",
                table: "SalesOrders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "SalesOrders",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SyncStatus",
                table: "SalesOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SalesOrderLines",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Products",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Mrp",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "Products",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Uom",
                table: "Products",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Products",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "Customers",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "Customers",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AlternateMobileNumber",
                table: "Customers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CoordinateCaptureSource",
                table: "Customers",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CoordinateCaptureTimestamp",
                table: "Customers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Customers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DistributorId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GstNumber",
                table: "Customers",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Customers",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locality",
                table: "Customers",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Customers",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutletType",
                table: "Customers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "Customers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RouteId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Customers",
                type: "varchar(80)",
                maxLength: 80,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "TerritoryId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Customers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DistributorId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Distributors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DistributorCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DistributorName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactPerson = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MobileNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distributors", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SyncQueueItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityClientUuid = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PayloadJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastRetryAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncQueueItems", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Territories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TerritoryCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TerritoryName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TsiUserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Territories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UploadJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UploadType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedByUserId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorFilePath = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadJobs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SalesRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RouteCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RouteName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TerritoryId = table.Column<int>(type: "int", nullable: false),
                    DistributorId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesRoutes_Distributors_DistributorId",
                        column: x => x.DistributorId,
                        principalTable: "Distributors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesRoutes_Territories_TerritoryId",
                        column: x => x.TerritoryId,
                        principalTable: "Territories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DaySessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RepUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDayTimestampUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartDayLat = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    StartDayLong = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    EndDayTimestampUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndDayLat = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    EndDayLong = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    SelectedRouteId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClientGeneratedUuid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SyncStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaySessions_SalesRoutes_SelectedRouteId",
                        column: x => x.SelectedRouteId,
                        principalTable: "SalesRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RouteAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    RepUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteAssignments_SalesRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "SalesRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ClientGeneratedUuid",
                table: "Visits",
                column: "ClientGeneratedUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DaySessionId",
                table: "Visits",
                column: "DaySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_RouteId",
                table: "Visits",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_ClientGeneratedUuid",
                table: "SalesOrders",
                column: "ClientGeneratedUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_DaySessionId",
                table: "SalesOrders",
                column: "DaySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_DistributorId",
                table: "SalesOrders",
                column: "DistributorId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_RouteId",
                table: "SalesOrders",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_VisitId",
                table: "SalesOrders",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DistributorId",
                table: "Customers",
                column: "DistributorId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_RouteId",
                table: "Customers",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TerritoryId",
                table: "Customers",
                column: "TerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DaySessions_ClientGeneratedUuid",
                table: "DaySessions",
                column: "ClientGeneratedUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DaySessions_RepUserId_BusinessDate",
                table: "DaySessions",
                columns: new[] { "RepUserId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DaySessions_SelectedRouteId",
                table: "DaySessions",
                column: "SelectedRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Distributors_DistributorCode",
                table: "Distributors",
                column: "DistributorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteAssignments_RepUserId_IsActive",
                table: "RouteAssignments",
                columns: new[] { "RepUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RouteAssignments_RouteId_IsActive",
                table: "RouteAssignments",
                columns: new[] { "RouteId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesRoutes_DistributorId",
                table: "SalesRoutes",
                column: "DistributorId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesRoutes_RouteCode",
                table: "SalesRoutes",
                column: "RouteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesRoutes_TerritoryId",
                table: "SalesRoutes",
                column: "TerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueueItems_EntityClientUuid",
                table: "SyncQueueItems",
                column: "EntityClientUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncQueueItems_SyncStatus",
                table: "SyncQueueItems",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Territories_TerritoryCode",
                table: "Territories",
                column: "TerritoryCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Distributors_DistributorId",
                table: "Customers",
                column: "DistributorId",
                principalTable: "Distributors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_SalesRoutes_RouteId",
                table: "Customers",
                column: "RouteId",
                principalTable: "SalesRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Territories_TerritoryId",
                table: "Customers",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_DaySessions_DaySessionId",
                table: "SalesOrders",
                column: "DaySessionId",
                principalTable: "DaySessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Distributors_DistributorId",
                table: "SalesOrders",
                column: "DistributorId",
                principalTable: "Distributors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_SalesRoutes_RouteId",
                table: "SalesOrders",
                column: "RouteId",
                principalTable: "SalesRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Visits_VisitId",
                table: "SalesOrders",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_DaySessions_DaySessionId",
                table: "Visits",
                column: "DaySessionId",
                principalTable: "DaySessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_SalesRoutes_RouteId",
                table: "Visits",
                column: "RouteId",
                principalTable: "SalesRoutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Distributors_DistributorId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_SalesRoutes_RouteId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Territories_TerritoryId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_DaySessions_DaySessionId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Distributors_DistributorId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_SalesRoutes_RouteId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Visits_VisitId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_DaySessions_DaySessionId",
                table: "Visits");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_SalesRoutes_RouteId",
                table: "Visits");

            migrationBuilder.DropTable(
                name: "DaySessions");

            migrationBuilder.DropTable(
                name: "RouteAssignments");

            migrationBuilder.DropTable(
                name: "SyncQueueItems");

            migrationBuilder.DropTable(
                name: "UploadJobs");

            migrationBuilder.DropTable(
                name: "SalesRoutes");

            migrationBuilder.DropTable(
                name: "Distributors");

            migrationBuilder.DropTable(
                name: "Territories");

            migrationBuilder.DropIndex(
                name: "IX_Visits_ClientGeneratedUuid",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_DaySessionId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_RouteId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_ClientGeneratedUuid",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_DaySessionId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_DistributorId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_RouteId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_VisitId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_DistributorId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_RouteId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TerritoryId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CheckinLat",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckinLong",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckinTimestampUtc",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckoutLat",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckoutLong",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckoutTimestampUtc",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedUuid",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CoordinateCapturedDuringVisitFlag",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CustomerRefLat",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CustomerRefLong",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "DaySessionId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "GeoDistanceMeters",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "RepUserId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "SyncStatus",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VisitStatus",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "WithinToleranceFlag",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ClientGeneratedUuid",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "DaySessionId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "DistributorId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "RepUserId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "SyncStatus",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Mrp",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Uom",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AlternateMobileNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CoordinateCaptureSource",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CoordinateCaptureTimestamp",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DistributorId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "GstNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Locality",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OutletType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RouteId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TerritoryId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DistributorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "SalesOrderLines",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");
        }
    }
}
