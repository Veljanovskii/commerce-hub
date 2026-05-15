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
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (dbContext.Products.Any())
        {
            return;
        }

        // Categories
        Category electronics = new() { Id = Guid.NewGuid(), Name = "Electronics" };
        Category computers = new() { Id = Guid.NewGuid(), Name = "Computers", ParentCategoryId = electronics.Id };
        Category phones = new() { Id = Guid.NewGuid(), Name = "Phones", ParentCategoryId = electronics.Id };
        Category clothing = new() { Id = Guid.NewGuid(), Name = "Clothing" };
        Category menClothing = new() { Id = Guid.NewGuid(), Name = "Men's Clothing", ParentCategoryId = clothing.Id };
        Category womenClothing = new() { Id = Guid.NewGuid(), Name = "Women's Clothing", ParentCategoryId = clothing.Id };
        Category home = new() { Id = Guid.NewGuid(), Name = "Home & Garden" };

        Category[] categories = [electronics, computers, phones, clothing, menClothing, womenClothing, home];
        dbContext.Categories.AddRange(categories);

        // Suppliers
        var suppliers = Enumerable.Range(1, 5).Select(i => new Supplier
        {
            Id = Guid.NewGuid(),
            Name = $"Supplier {i}",
            ContactEmail = $"contact@supplier{i}.com",
            ContactPhone = $"+1-555-000{i}"
        }).ToList();
        dbContext.Suppliers.AddRange(suppliers);

        // Products (~50)
        Category[] productCategories = [computers, computers, phones, phones, menClothing, womenClothing, home];
        List<Product> products = [];
        for (int i = 1; i <= 50; i++)
        {
            Category cat = productCategories[i % productCategories.Length];
            products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Sku = $"SKU-{i:D5}",
                Description = $"Description for product {i}. High-quality item in the {cat.Name} category.",
                Price = Math.Round(10m + i * 3.5m, 2),
                CategoryId = cat.Id
            });
        }
        dbContext.Products.AddRange(products);

        // StockItems
        List<StockItem> stockItems = [];
        for (int i = 0; i < products.Count; i++)
        {
            stockItems.Add(new StockItem
            {
                Id = Guid.NewGuid(),
                ProductId = products[i].Id,
                SupplierId = suppliers[i % suppliers.Count].Id,
                QuantityOnHand = 50 + i * 3,
                ReorderLevel = 10
            });
            if (i % 3 == 0)
            {
                stockItems.Add(new StockItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = products[i].Id,
                    SupplierId = suppliers[(i + 1) % suppliers.Count].Id,
                    QuantityOnHand = 20 + i,
                    ReorderLevel = 5
                });
            }
        }
        dbContext.StockItems.AddRange(stockItems);

        // Customers
        var customers = Enumerable.Range(1, 10).Select(i => new Customer
        {
            Id = Guid.NewGuid(),
            Name = $"Customer {i}",
            Email = $"customer{i}@example.com",
            Addresses =
            [
                new Address
                {
                    Id = Guid.NewGuid(),
                    Street = $"{100 + i} Main St",
                    City = "Anytown",
                    PostalCode = $"{10000 + i}",
                    Country = "US"
                }
            ]
        }).ToList();
        dbContext.Customers.AddRange(customers);

        // Orders (~20)
        List<Order> orders = [];
        for (int i = 0; i < 20; i++)
        {
            Customer customer = customers[i % customers.Count];
            List<OrderLine> orderLines = [];
            int lineCount = 1 + i % 4;
            for (int j = 0; j < lineCount; j++)
            {
                Product product = products[(i * 3 + j) % products.Count];
                orderLines.Add(new OrderLine
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Quantity = 1 + j % 3,
                    UnitPriceAtOrder = product.Price
                });
            }

            Order order = new()
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Status = (OrderStatus)(i % 5),
                PlacedAt = DateTime.UtcNow.AddDays(-20 + i),
                Total = orderLines.Sum(l => l.Quantity * l.UnitPriceAtOrder),
                OrderLines = orderLines
            };
            orders.Add(order);
        }
        dbContext.Orders.AddRange(orders);

        dbContext.SaveChanges();
    }
}
