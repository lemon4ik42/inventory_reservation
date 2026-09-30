using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryReservation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RichInventoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WarehousePriority",
                table: "StockItems");

            migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "StockReservations",
            type: "boolean",
            nullable: false,
            defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE "StockReservations" AS reservation
                SET "IsActive" = EXISTS (
                    SELECT 1 FROM "SalesOrders" AS orders
                    WHERE orders."Id" = reservation."SalesOrderId"
                      AND orders."Status" = 2
                );
                """);

            migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "SalesOrders",
            type: "bytea",
            nullable: false,
            defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
            name: "IX_StockReservations_StockItemId",
            table: "StockReservations",
            column: "StockItemId");

            migrationBuilder.CreateIndex(
            name: "IX_StockItems_WarehouseId",
            table: "StockItems",
            column: "WarehouseId");

            migrationBuilder.CreateIndex(
            name: "IX_InventoryMovements_StockItemId",
            table: "InventoryMovements",
            column: "StockItemId");

            migrationBuilder.AddForeignKey(
            name: "FK_InventoryMovements_StockItems_StockItemId",
            table: "InventoryMovements",
            column: "StockItemId",
            principalTable: "StockItems",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
            name: "FK_StockItems_Products_ProductId",
            table: "StockItems",
            column: "ProductId",
            principalTable: "Products",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
            name: "FK_StockItems_Warehouses_WarehouseId",
            table: "StockItems",
            column: "WarehouseId",
            principalTable: "Warehouses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
            name: "FK_StockReservations_StockItems_StockItemId",
            table: "StockReservations",
            column: "StockItemId",
            principalTable: "StockItems",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
            name: "FK_InventoryMovements_StockItems_StockItemId",
            table: "InventoryMovements");

            migrationBuilder.DropForeignKey(
            name: "FK_StockItems_Products_ProductId",
            table: "StockItems");

            migrationBuilder.DropForeignKey(
            name: "FK_StockItems_Warehouses_WarehouseId",
            table: "StockItems");

            migrationBuilder.DropForeignKey(
            name: "FK_StockReservations_StockItems_StockItemId",
            table: "StockReservations");

            migrationBuilder.DropIndex(
            name: "IX_StockReservations_StockItemId",
            table: "StockReservations");

            migrationBuilder.DropIndex(
            name: "IX_StockItems_WarehouseId",
            table: "StockItems");

            migrationBuilder.DropIndex(
            name: "IX_InventoryMovements_StockItemId",
            table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SalesOrders");

            migrationBuilder.AddColumn<int>(
            name: "WarehousePriority",
            table: "StockItems",
            type: "integer",
            nullable: false,
            defaultValue: 0);
        }
    }
}

