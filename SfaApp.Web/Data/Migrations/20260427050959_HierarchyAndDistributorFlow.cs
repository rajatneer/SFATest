using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SfaApp.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class HierarchyAndDistributorFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO `Distributors` (`DistributorCode`, `DistributorName`, `IsActive`)
                SELECT 'DIST-MIG', 'Migration Distributor', 1
                WHERE NOT EXISTS (SELECT 1 FROM `Distributors`);

                INSERT INTO `Territories` (`TerritoryCode`, `TerritoryName`, `TsiUserId`, `IsActive`)
                SELECT 'TERR-MIG', 'Migration Territory', NULL, 1
                WHERE NOT EXISTS (SELECT 1 FROM `Territories`);

                INSERT INTO `SalesRoutes` (`RouteCode`, `RouteName`, `TerritoryId`, `DistributorId`, `IsActive`)
                SELECT 'RTE-MIG',
                       'Migration Route',
                       (SELECT `Id` FROM `Territories` ORDER BY `Id` LIMIT 1),
                       (SELECT `Id` FROM `Distributors` ORDER BY `Id` LIMIT 1),
                       1
                WHERE NOT EXISTS (SELECT 1 FROM `SalesRoutes`);

                UPDATE `Customers`
                SET `RouteId` = (SELECT `Id` FROM `SalesRoutes` ORDER BY `Id` LIMIT 1)
                WHERE `RouteId` IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "RouteId",
                table: "Customers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerUserId",
                table: "AspNetUsers",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagerUserId",
                table: "AspNetUsers",
                column: "ManagerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerUserId",
                table: "AspNetUsers",
                column: "ManagerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ManagerUserId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagerUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "RouteId",
                table: "Customers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
