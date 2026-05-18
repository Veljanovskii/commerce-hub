using System.Globalization;
using Application.Abstractions.Authentication;
using Bogus;
using Domain.Catalog;
using Domain.Customers;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CA5394 // Random is insecure

namespace Infrastructure.Database;

public sealed class SeedDataGenerator(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher)
{
    public async Task GenerateAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        // Generate test data for our benchmarks
        int numCategories = 20;
        int numSuppliers = 50;
        int numProducts = 1000;
        int numCustomers = 500;
        int numOrders = 2000;

        Faker<Category> categoryFaker = new Faker<Category>()
            .RuleFor(c => c.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0] + " " + f.Random.Word() + " " + f.Random.Int(1, 10000))
            .RuleFor(c => c.Description, f => f.Commerce.ProductDescription());

        List<Category> categories = categoryFaker.Generate(numCategories);
        await dbContext.Categories.AddRangeAsync(categories, cancellationToken);

        Faker<Supplier> supplierFaker = new Faker<Supplier>()
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.CompanyName, f => f.Company.CompanyName() + " " + f.Random.Int(1, 10000))
            .RuleFor(s => s.ContactName, f => f.Name.FullName())
            .RuleFor(s => s.Email, (f, s) => f.Internet.Email(s.ContactName))
            .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(s => s.Address, f => f.Address.StreetAddress())
            .RuleFor(s => s.City, f => f.Address.City())
            .RuleFor(s => s.Country, f => f.Address.Country());

        List<Supplier> suppliers = supplierFaker.Generate(numSuppliers);
        await dbContext.Suppliers.AddRangeAsync(suppliers, cancellationToken);

        Faker<Product> productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName() + " " + f.Random.Int(1, 10000))
            .RuleFor(p => p.Sku, f => f.Commerce.Ean13() + "-" + f.Random.Int(1, 10000))
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.UnitPrice, f => decimal.Parse(f.Commerce.Price(1, 1000), CultureInfo.InvariantCulture))
            .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 1000))
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(p => p.SupplierId, f => f.PickRandom(suppliers).Id)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(2).ToUniversalTime());

        List<Product> products = productFaker.Generate(numProducts);
        await dbContext.Products.AddRangeAsync(products, cancellationToken);

        Faker<Customer> customerFaker = new Faker<Customer>()
            .RuleFor(c => c.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.Email, f => f.Internet.Email() + f.Random.Int(1, 10000))
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.PasswordHash, _ => passwordHasher.Hash("Password123!"))
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Address, f => f.Address.StreetAddress())
            .RuleFor(c => c.City, f => f.Address.City())
            .RuleFor(c => c.Country, f => f.Address.Country())
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(2).ToUniversalTime());

        List<Customer> customers = customerFaker.Generate(numCustomers);
        await dbContext.Customers.AddRangeAsync(customers, cancellationToken);

        Faker<Order> orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, _ => Guid.NewGuid())
            .RuleFor(o => o.CustomerId, f => f.PickRandom(customers).Id)
            .RuleFor(o => o.OrderDate, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
            .RuleFor(o => o.ShippingAddress, f => f.Address.StreetAddress())
            .RuleFor(o => o.ShippingCity, f => f.Address.City())
            .RuleFor(o => o.ShippingCountry, f => f.Address.Country());

        List<Order> orders = orderFaker.Generate(numOrders);

        foreach (Order order in orders)
        {
            int numItems = Random.Shared.Next(1, 6);
            var selectedProducts = new Faker().PickRandom(products, numItems).ToList();

            decimal totalAmount = 0;

            foreach (Product product in selectedProducts)
            {
                int quantity = Random.Shared.Next(1, 5);
                decimal discount = new Faker().Random.Bool(0.2f) ? Math.Round(product.UnitPrice * 0.1m, 2) : 0;
                decimal lineTotal = product.UnitPrice * quantity - discount;

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.UnitPrice,
                    Discount = discount
                };

                order.Items.Add(orderItem);
                totalAmount += lineTotal;
            }

            order.TotalAmount = totalAmount;
        }

        await dbContext.Orders.AddRangeAsync(orders, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
