using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialCommerceMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "public");

        migrationBuilder.CreateTable(
            name: "categories",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                parent_category_id = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_categories", x => x.id);
                table.ForeignKey(
                    name: "fk_categories_categories_parent_category_id",
                    column: x => x.parent_category_id,
                    principalSchema: "public",
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "customers",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_customers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "suppliers",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_suppliers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                shipping_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                shipping_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                shipping_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_orders", x => x.id);
                table.ForeignKey(
                    name: "fk_orders_customers_customer_id",
                    column: x => x.customer_id,
                    principalSchema: "public",
                    principalTable: "customers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "products",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                stock_quantity = table.Column<int>(type: "integer", nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_products", x => x.id);
                table.ForeignKey(
                    name: "fk_products_categories_category_id",
                    column: x => x.category_id,
                    principalSchema: "public",
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_products_suppliers_supplier_id",
                    column: x => x.supplier_id,
                    principalSchema: "public",
                    principalTable: "suppliers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "order_items",
            schema: "public",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_order_items", x => x.id);
                table.ForeignKey(
                    name: "fk_order_items_orders_order_id",
                    column: x => x.order_id,
                    principalSchema: "public",
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_order_items_products_product_id",
                    column: x => x.product_id,
                    principalSchema: "public",
                    principalTable: "products",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_categories_name",
            schema: "public",
            table: "categories",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_categories_parent_category_id",
            schema: "public",
            table: "categories",
            column: "parent_category_id");

        migrationBuilder.CreateIndex(
            name: "ix_customers_email",
            schema: "public",
            table: "customers",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_order_items_order_id",
            schema: "public",
            table: "order_items",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "ix_order_items_product_id",
            schema: "public",
            table: "order_items",
            column: "product_id");

        migrationBuilder.CreateIndex(
            name: "ix_orders_customer_id",
            schema: "public",
            table: "orders",
            column: "customer_id");

        migrationBuilder.CreateIndex(
            name: "ix_orders_order_date",
            schema: "public",
            table: "orders",
            column: "order_date");

        migrationBuilder.CreateIndex(
            name: "ix_orders_status",
            schema: "public",
            table: "orders",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_products_category_id",
            schema: "public",
            table: "products",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_products_sku",
            schema: "public",
            table: "products",
            column: "sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_products_supplier_id",
            schema: "public",
            table: "products",
            column: "supplier_id");

        migrationBuilder.CreateIndex(
            name: "ix_suppliers_email",
            schema: "public",
            table: "suppliers",
            column: "email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "order_items",
            schema: "public");

        migrationBuilder.DropTable(
            name: "orders",
            schema: "public");

        migrationBuilder.DropTable(
            name: "products",
            schema: "public");

        migrationBuilder.DropTable(
            name: "customers",
            schema: "public");

        migrationBuilder.DropTable(
            name: "categories",
            schema: "public");

        migrationBuilder.DropTable(
            name: "suppliers",
            schema: "public");
    }
}
