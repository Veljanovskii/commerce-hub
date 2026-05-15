using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations;

/// <summary>
/// Creates history tables and triggers for PostgreSQL temporal-style versioning
/// on orders, products, and stock_items tables.
/// </summary>
public partial class AddTemporalHistoryTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Orders history table
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS public.orders_history (LIKE public.orders INCLUDING ALL);
            ALTER TABLE public.orders_history DROP CONSTRAINT IF EXISTS pk_orders;
            ALTER TABLE public.orders_history ADD PRIMARY KEY (id, valid_from);
            """);

        // Products history table
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS public.products_history (LIKE public.products INCLUDING ALL);
            ALTER TABLE public.products_history DROP CONSTRAINT IF EXISTS pk_products;
            ALTER TABLE public.products_history ADD PRIMARY KEY (id, valid_from);
            """);

        // Stock items history table
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS public.stock_items_history (LIKE public.stock_items INCLUDING ALL);
            ALTER TABLE public.stock_items_history DROP CONSTRAINT IF EXISTS pk_stock_items;
            ALTER TABLE public.stock_items_history ADD PRIMARY KEY (id, valid_from);
            """);

        // Trigger function for versioning
        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION temporal_versioning()
            RETURNS TRIGGER AS $$
            BEGIN
                IF TG_OP = 'UPDATE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.{table_name}_history SELECT OLD.*;
                    NEW.valid_from := now() at time zone 'utc';
                    NEW.valid_to := '9999-12-31T23:59:59'::timestamp;
                    RETURN NEW;
                ELSIF TG_OP = 'DELETE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.{table_name}_history SELECT OLD.*;
                    RETURN OLD;
                END IF;
                RETURN NULL;
            END;
            $$ LANGUAGE plpgsql;
            """);

        // Create specific trigger functions and attach triggers
        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION orders_versioning()
            RETURNS TRIGGER AS $$
            BEGIN
                IF TG_OP = 'UPDATE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.orders_history SELECT OLD.*;
                    NEW.valid_from := now() at time zone 'utc';
                    NEW.valid_to := '9999-12-31T23:59:59'::timestamp;
                    RETURN NEW;
                ELSIF TG_OP = 'DELETE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.orders_history SELECT OLD.*;
                    RETURN OLD;
                END IF;
                RETURN NULL;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE TRIGGER trg_orders_versioning
            BEFORE UPDATE OR DELETE ON public.orders
            FOR EACH ROW EXECUTE FUNCTION orders_versioning();
            """);

        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION products_versioning()
            RETURNS TRIGGER AS $$
            BEGIN
                IF TG_OP = 'UPDATE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.products_history SELECT OLD.*;
                    NEW.valid_from := now() at time zone 'utc';
                    NEW.valid_to := '9999-12-31T23:59:59'::timestamp;
                    RETURN NEW;
                ELSIF TG_OP = 'DELETE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.products_history SELECT OLD.*;
                    RETURN OLD;
                END IF;
                RETURN NULL;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE TRIGGER trg_products_versioning
            BEFORE UPDATE OR DELETE ON public.products
            FOR EACH ROW EXECUTE FUNCTION products_versioning();
            """);

        migrationBuilder.Sql("""
            CREATE OR REPLACE FUNCTION stock_items_versioning()
            RETURNS TRIGGER AS $$
            BEGIN
                IF TG_OP = 'UPDATE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.stock_items_history SELECT OLD.*;
                    NEW.valid_from := now() at time zone 'utc';
                    NEW.valid_to := '9999-12-31T23:59:59'::timestamp;
                    RETURN NEW;
                ELSIF TG_OP = 'DELETE' THEN
                    OLD.valid_to := now() at time zone 'utc';
                    INSERT INTO public.stock_items_history SELECT OLD.*;
                    RETURN OLD;
                END IF;
                RETURN NULL;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE TRIGGER trg_stock_items_versioning
            BEFORE UPDATE OR DELETE ON public.stock_items
            FOR EACH ROW EXECUTE FUNCTION stock_items_versioning();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_orders_versioning ON public.orders;");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_products_versioning ON public.products;");
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_stock_items_versioning ON public.stock_items;");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS orders_versioning();");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS products_versioning();");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS stock_items_versioning();");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS temporal_versioning();");
        migrationBuilder.Sql("DROP TABLE IF EXISTS public.orders_history;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS public.products_history;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS public.stock_items_history;");
    }
}
