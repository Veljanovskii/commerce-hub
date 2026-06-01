using Domain.Customers;
using Domain.Orders;
using Domain.Products;
using Domain.Supplies;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seeding;

public static class DataSeeder
{
    private const int SupplierCount = 25;
    private const int ProductCount = 1_000;
    private const int CustomerCount = 500;
    private const int OrderCount = 5_000;

    public static void SeedData(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (dbContext.Products.Any() || dbContext.Customers.Any())
        {
            return;
        }

        bool originalAutoDetectChanges = dbContext.ChangeTracker.AutoDetectChangesEnabled;
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            DateTime baseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Categories
            Category electronics = new() { Id = Guid.NewGuid(), Name = "Electronics" };
            Category computers = new() { Id = Guid.NewGuid(), Name = "Computers", ParentCategoryId = electronics.Id };
            Category laptops = new() { Id = Guid.NewGuid(), Name = "Laptops", ParentCategoryId = computers.Id };
            Category desktops = new() { Id = Guid.NewGuid(), Name = "Desktops", ParentCategoryId = computers.Id };
            Category monitors = new() { Id = Guid.NewGuid(), Name = "Monitors", ParentCategoryId = computers.Id };
            Category phones = new() { Id = Guid.NewGuid(), Name = "Phones", ParentCategoryId = electronics.Id };
            Category smartphones = new() { Id = Guid.NewGuid(), Name = "Smartphones", ParentCategoryId = phones.Id };
            Category accessories = new() { Id = Guid.NewGuid(), Name = "Accessories", ParentCategoryId = electronics.Id };

            Category clothing = new() { Id = Guid.NewGuid(), Name = "Clothing" };
            Category menClothing = new() { Id = Guid.NewGuid(), Name = "Men's Clothing", ParentCategoryId = clothing.Id };
            Category womenClothing = new() { Id = Guid.NewGuid(), Name = "Women's Clothing", ParentCategoryId = clothing.Id };
            Category shoes = new() { Id = Guid.NewGuid(), Name = "Shoes", ParentCategoryId = clothing.Id };
            Category sportswear = new() { Id = Guid.NewGuid(), Name = "Sportswear", ParentCategoryId = clothing.Id };

            Category home = new() { Id = Guid.NewGuid(), Name = "Home & Garden" };
            Category kitchen = new() { Id = Guid.NewGuid(), Name = "Kitchen", ParentCategoryId = home.Id };
            Category furniture = new() { Id = Guid.NewGuid(), Name = "Furniture", ParentCategoryId = home.Id };
            Category garden = new() { Id = Guid.NewGuid(), Name = "Garden", ParentCategoryId = home.Id };

            Category books = new() { Id = Guid.NewGuid(), Name = "Books" };
            Category fiction = new() { Id = Guid.NewGuid(), Name = "Fiction", ParentCategoryId = books.Id };
            Category nonFiction = new() { Id = Guid.NewGuid(), Name = "Non-fiction", ParentCategoryId = books.Id };

            Category[] categories =
            [
                electronics, computers, laptops, desktops, monitors, phones, smartphones, accessories,
                clothing, menClothing, womenClothing, shoes, sportswear,
                home, kitchen, furniture, garden,
                books, fiction, nonFiction
            ];

            dbContext.Categories.AddRange(categories);

            // Suppliers
            var suppliers = Enumerable.Range(1, SupplierCount)
                .Select(i => new Supplier
                {
                    Id = Guid.NewGuid(),
                    Name = $"Supplier {i:D2}",
                    ContactEmail = $"contact@supplier{i:D2}.com",
                    ContactPhone = $"+1-555-{i:D4}"
                })
                .ToList();

            dbContext.Suppliers.AddRange(suppliers);

            // Products
            Category[] productCategories =
            [
                laptops, desktops, monitors, smartphones, accessories,
                menClothing, womenClothing, shoes, sportswear,
                kitchen, furniture, garden,
                fiction, nonFiction
            ];

            List<Product> products = [];

            for (int i = 1; i <= ProductCount; i++)
            {
                Category category = productCategories[i % productCategories.Length];

                decimal price = Math.Round(
                    10m + i % 500 * 2.75m + i % 7 * 0.99m,
                    2);

                products.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    Name = $"Product {i:D4}",
                    Sku = $"SKU-{i:D6}",
                    Description = $"Description for product {i:D4}. High-quality item in the {category.Name} category.",
                    Price = price,
                    CategoryId = category.Id
                });
            }

            dbContext.Products.AddRange(products);

            // StockItems
            List<StockItem> stockItems = [];

            for (int i = 0; i < products.Count; i++)
            {
                Product product = products[i];

                stockItems.Add(new StockItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    SupplierId = suppliers[i % suppliers.Count].Id,
                    QuantityOnHand = 50 + i % 250,
                    ReorderLevel = 10 + i % 20
                });

                if (i % 3 == 0)
                {
                    stockItems.Add(new StockItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        SupplierId = suppliers[(i + 1) % suppliers.Count].Id,
                        QuantityOnHand = 25 + i % 150,
                        ReorderLevel = 5 + i % 10
                    });
                }

                if (i % 10 == 0)
                {
                    stockItems.Add(new StockItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        SupplierId = suppliers[(i + 2) % suppliers.Count].Id,
                        QuantityOnHand = 10 + i % 100,
                        ReorderLevel = 5
                    });
                }
            }

            dbContext.StockItems.AddRange(stockItems);

            // Customers
            var customers = Enumerable.Range(1, CustomerCount)
                .Select(i => new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = $"Customer {i:D4}",
                    Email = $"customer{i:D4}@example.com",
                    Addresses =
                    [
                        new Address
                        {
                            Id = Guid.NewGuid(),
                            Street = $"{100 + i} Main St",
                            City = $"City {i % 20 + 1}",
                            PostalCode = $"{10000 + i}",
                            Country = "US"
                        }
                    ]
                })
                .ToList();

            dbContext.Customers.AddRange(customers);

            // Orders
            List<Order> orders = [];

            for (int i = 0; i < OrderCount; i++)
            {
                Customer customer = customers[i % customers.Count];

                List<OrderLine> orderLines = [];

                int lineCount = 2 + i % 4; // 2-5 lines per order

                for (int j = 0; j < lineCount; j++)
                {
                    Product product = products[(i * 7 + j * 13) % products.Count];

                    orderLines.Add(new OrderLine
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Quantity = 1 + j % 4,
                        UnitPriceAtOrder = product.Price
                    });
                }

                Order order = new()
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Status = (OrderStatus)(i % 5),
                    PlacedAt = baseDate.AddMinutes(-i * 15),
                    Total = orderLines.Sum(l => l.Quantity * l.UnitPriceAtOrder),
                    OrderLines = orderLines
                };

                orders.Add(order);
            }

            dbContext.Orders.AddRange(orders);

            dbContext.SaveChanges();
        }
        finally
        {
            dbContext.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetectChanges;
        }
    }
}
