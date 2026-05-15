using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTemporalColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "valid_from",
                schema: "public",
                table: "stock_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_to",
                schema: "public",
                table: "stock_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "'9999-12-31T23:59:59'::timestamp");

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_from",
                schema: "public",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_to",
                schema: "public",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "'9999-12-31T23:59:59'::timestamp");

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_from",
                schema: "public",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_to",
                schema: "public",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "'9999-12-31T23:59:59'::timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "valid_from",
                schema: "public",
                table: "stock_items");

            migrationBuilder.DropColumn(
                name: "valid_to",
                schema: "public",
                table: "stock_items");

            migrationBuilder.DropColumn(
                name: "valid_from",
                schema: "public",
                table: "products");

            migrationBuilder.DropColumn(
                name: "valid_to",
                schema: "public",
                table: "products");

            migrationBuilder.DropColumn(
                name: "valid_from",
                schema: "public",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "valid_to",
                schema: "public",
                table: "orders");
        }
    }
}
