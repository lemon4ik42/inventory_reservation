using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace InventoryReservation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc/>
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "Customers", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), Name = table.Column<string>(type: "text", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "InventoryMovements", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), StockItemId = table.Column<Guid>(type: "uuid", nullable: false), Type = table.Column<int>(type: "integer", nullable: false), Quantity = table.Column<int>(type: "integer", nullable: false), ApplicationUserId = table.Column<Guid>(type: "uuid", nullable: false), SalesOrderId = table.Column<Guid>(type: "uuid", nullable: true), OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_InventoryMovements", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Products", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false), Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false), Category = table.Column<string>(type: "text", nullable: false), SupplierId = table.Column<Guid>(type: "uuid", nullable: false), IsActive = table.Column<bool>(type: "boolean", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "SalesOrders", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), Number = table.Column<string>(type: "text", nullable: false), CustomerId = table.Column<Guid>(type: "uuid", nullable: false), Status = table.Column<int>(type: "integer", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrders", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "StockItems", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), ProductId = table.Column<Guid>(type: "uuid", nullable: false), WarehouseId = table.Column<Guid>(type: "uuid", nullable: false), WarehousePriority = table.Column<int>(type: "integer", nullable: false), OnHandQuantity = table.Column<int>(type: "integer", nullable: false), ReservedQuantity = table.Column<int>(type: "integer", nullable: false), RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_StockItems", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Suppliers", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), Name = table.Column<string>(type: "text", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Suppliers", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Users", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), UserName = table.Column<string>(type: "text", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "Warehouses", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false), Address = table.Column<string>(type: "text", nullable: false), Priority = table.Column<int>(type: "integer", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_Warehouses", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "SalesOrderItems", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false), ProductId = table.Column<Guid>(type: "uuid", nullable: false), Quantity = table.Column<int>(type: "integer", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_SalesOrderItems", x => x.Id);
                table.ForeignKey(name: "FK_SalesOrderItems_SalesOrders_SalesOrderId", column: x => x.SalesOrderId, principalTable: "SalesOrders", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "StockReservations", columns: table => new { Id = table.Column<Guid>(type: "uuid", nullable: false), SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false), SalesOrderItemId = table.Column<Guid>(type: "uuid", nullable: false), StockItemId = table.Column<Guid>(type: "uuid", nullable: false), Quantity = table.Column<int>(type: "integer", nullable: false), CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_StockReservations", x => x.Id);
                table.ForeignKey(name: "FK_StockReservations_SalesOrders_SalesOrderId", column: x => x.SalesOrderId, principalTable: "SalesOrders", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.InsertData(
            table: "Customers",
            columns: new[] { "Id", "Name" },
            values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "Demo customer" });
            migrationBuilder.InsertData(
            table: "Products",
            columns: new[] { "Id", "Category", "IsActive", "Name", "Sku", "SupplierId" },
            values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), "Demo", true, "Demo product", "DEMO-001", new Guid("11111111-1111-1111-1111-111111111111") });
            migrationBuilder.InsertData(
            table: "Suppliers",
            columns: new[] { "Id", "Name" },
            values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Demo supplier" });
            migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "Id", "UserName" },
            values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), "warehouse.demo" });
            migrationBuilder.InsertData(
            table: "Warehouses",
            columns: new[] { "Id", "Address", "Name", "Priority" },
            values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), "Demo street, 1", "Main warehouse", 100 });
            migrationBuilder.CreateIndex(name: "IX_Products_Sku", table: "Products", column: "Sku", unique: true);
            migrationBuilder.CreateIndex(
            name: "IX_SalesOrderItems_SalesOrderId",
            table: "SalesOrderItems",
            column: "SalesOrderId");
            migrationBuilder.CreateIndex(
            name: "IX_StockItems_ProductId_WarehouseId",
            table: "StockItems",
            columns: new[] { "ProductId", "WarehouseId" },
            unique: true);
            migrationBuilder.CreateIndex(
            name: "IX_StockReservations_SalesOrderId",
            table: "StockReservations",
            column: "SalesOrderId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Customers");
            migrationBuilder.DropTable(name: "InventoryMovements");
            migrationBuilder.DropTable(name: "Products");
            migrationBuilder.DropTable(name: "SalesOrderItems");
            migrationBuilder.DropTable(name: "StockItems");
            migrationBuilder.DropTable(name: "StockReservations");
            migrationBuilder.DropTable(name: "Suppliers");
            migrationBuilder.DropTable(name: "Users");
            migrationBuilder.DropTable(name: "Warehouses");
            migrationBuilder.DropTable(name: "SalesOrders");
        }
    }
}

