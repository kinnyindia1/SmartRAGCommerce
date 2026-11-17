
using Microsoft.EntityFrameworkCore;
using SmartRAGCommerce.Infrastructure.Persistence.Context;

namespace SmartRAGCommerce.Infrastructure.Persistence.Seed;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;

    public DatabaseSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Avoid inserting duplicate categories
        if (await _context.Categories.AnyAsync())
            return;

        var categories = new List<Domain.Entities.Category>
        {
            new() { Name = "Electronics", Description = "Electronic devices and accessories" },
            new() { Name = "Mobiles", Description = "Mobile phones and accessories" },
            new() { Name = "Computers", Description = "Laptops and computer accessories" },
            new() { Name = "Home Appliances", Description = "Appliances for home use" },
            new() { Name = "Fashion", Description = "Clothing and fashion accessories" },
            new() { Name = "Books", Description = "Books and educational materials" },
            new() { Name = "Furniture", Description = "Home and office furniture" },
            new() { Name = "Sports", Description = "Sports and fitness products" }
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();


        //var categories = await _context.Categories.ToListAsync();

        var products = new List<Domain.Entities.Product>
        {
            new() { Name = "Samsung Galaxy S25", Description = "Samsung smartphone", Price = 79999, StockQuantity = 25, CategoryId = categories.First(c => c.Name == "Mobiles").Id },
            new() { Name = "iPhone 16", Description = "Apple smartphone", Price = 69999, StockQuantity = 30, CategoryId = categories.First(c => c.Name == "Mobiles").Id },
            new() { Name = "Dell Inspiron Laptop", Description = "Dell laptop", Price = 65000, StockQuantity = 15, CategoryId = categories.First(c => c.Name == "Computers").Id },
            new() { Name = "HP Pavilion Laptop", Description = "HP laptop", Price = 72000, StockQuantity = 12, CategoryId = categories.First(c => c.Name == "Computers").Id },
            new() { Name = "Sony Headphones", Description = "Wireless headphones", Price = 4999, StockQuantity = 40, CategoryId = categories.First(c => c.Name == "Electronics").Id },
            new() { Name = "Samsung 55 Inch TV", Description = "Smart LED television", Price = 55000, StockQuantity = 10, CategoryId = categories.First(c => c.Name == "Electronics").Id },
            new() { Name = "LG Washing Machine", Description = "Front load washing machine", Price = 32000, StockQuantity = 8, CategoryId = categories.First(c => c.Name == "Home Appliances").Id },
            new() { Name = "Nike Running Shoes", Description = "Sports running shoes", Price = 5999, StockQuantity = 35, CategoryId = categories.First(c => c.Name == "Sports").Id },
            new() { Name = "Wooden Study Table", Description = "Engineered wood study table", Price = 8500, StockQuantity = 20, CategoryId = categories.First(c => c.Name == "Furniture").Id },
            new() { Name = "C# Programming Book", Description = "Programming learning book", Price = 899, StockQuantity = 50, CategoryId = categories.First(c => c.Name == "Books").Id }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();



        var customers = new List<Domain.Entities.Customer>
        {
            new() { FirstName = "Arun", LastName = "Kumar", Email = "arun@example.com", PhoneNumber = "9876543210", Address = "Chennai" },
            new() { FirstName = "Priya", LastName = "Sharma", Email = "priya@example.com", PhoneNumber = "9876543211", Address = "Bangalore" },
            new() { FirstName = "Rahul", LastName = "Verma", Email = "rahul@example.com", PhoneNumber = "9876543212", Address = "Hyderabad" },
            new() { FirstName = "Divya", LastName = "Rajan", Email = "divya@example.com", PhoneNumber = "9876543213", Address = "Coimbatore" },
            new() { FirstName = "Karthik", LastName = "Sundar", Email = "karthik@example.com", PhoneNumber = "9876543214", Address = "Madurai" },
            new() { FirstName = "Meena", LastName = "Devi", Email = "meena@example.com", PhoneNumber = "9876543215", Address = "Kanchipuram" },
            new() { FirstName = "Vijay", LastName = "Raj", Email = "vijay@example.com", PhoneNumber = "9876543216", Address = "Salem" },
            new() { FirstName = "Sneha", LastName = "Patel", Email = "sneha@example.com", PhoneNumber = "9876543217", Address = "Mumbai" },
            new() { FirstName = "Suresh", LastName = "Babu", Email = "suresh@example.com", PhoneNumber = "9876543218", Address = "Trichy" },
            new() { FirstName = "Ananya", LastName = "Rao", Email = "ananya@example.com", PhoneNumber = "9876543219", Address = "Pune" }
        };

        await _context.Customers.AddRangeAsync(customers);
        await _context.SaveChangesAsync();


        var customersList = await _context.Customers.ToListAsync();
        var productsList = await _context.Products.ToListAsync();

        var random = new Random(42);
        var orders = new List<Domain.Entities.Order>();

        for (int i = 0; i < 30; i++)
        {
            var customer = customersList[i % customersList.Count];

            var order = new Domain.Entities.Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.UtcNow.AddDays(-random.Next(0, 31)),
                Status = i % 5 == 0 ? "Pending" : "Completed",
                TotalAmount = 0
            };

            int itemCount = random.Next(1, 4);

            for (int j = 0; j < itemCount; j++)
            {
                var product = productsList[random.Next(productsList.Count)];
                int quantity = random.Next(1, 4);

                order.OrderItems.Add(new Domain.Entities.OrderItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.Price
                });

                order.TotalAmount += product.Price * quantity;
            }

            orders.Add(order);
        }

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        var savedOrders = await _context.Orders.ToListAsync();

        var payments = new List<Domain.Entities.Payment>();

        foreach (var order in savedOrders)
        {
            // Pending orders remain unpaid
            if (order.Status == "Pending")
                continue;

            payments.Add(new Domain.Entities.Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethod = order.Id % 2 == 0 ? "Credit Card" : "UPI",
                PaymentStatus = "Completed",
                PaymentDate = order.OrderDate
            });
        }

        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();



    }
}
