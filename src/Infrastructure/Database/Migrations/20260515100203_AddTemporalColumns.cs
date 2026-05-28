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
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='stock_items' AND column_name='valid_from') THEN
                        ALTER TABLE public.stock_items ADD COLUMN valid_from timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc');
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='stock_items' AND column_name='valid_to') THEN
                        ALTER TABLE public.stock_items ADD COLUMN valid_to timestamp with time zone NOT NULL DEFAULT '9999-12-31T23:59:59'::timestamp;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='products' AND column_name='valid_from') THEN
                        ALTER TABLE public.products ADD COLUMN valid_from timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc');
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='products' AND column_name='valid_to') THEN
                        ALTER TABLE public.products ADD COLUMN valid_to timestamp with time zone NOT NULL DEFAULT '9999-12-31T23:59:59'::timestamp;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='orders' AND column_name='valid_from') THEN
                        ALTER TABLE public.orders ADD COLUMN valid_from timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc');
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='public' AND table_name='orders' AND column_name='valid_to') THEN
                        ALTER TABLE public.orders ADD COLUMN valid_to timestamp with time zone NOT NULL DEFAULT '9999-12-31T23:59:59'::timestamp;
                    END IF;
                END $$;
                """);
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
