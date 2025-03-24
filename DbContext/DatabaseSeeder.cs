using Backend.Models;

namespace Backend.DbContext;

public static class DatabaseSeeder
{
    public static void SeedData(AppDbContext context)
    {
        if (context.Products.Any()) return;

        // Create categories
        var categories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Home & Kitchen",
                Description = "Home appliances and kitchen essentials",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                Description = "Books across various genres",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Sports & Outdoors",
                Description = "Sports equipment and outdoor gear",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.AddRange(categories);
        context.SaveChanges();

        // Create products
        var products = new List<Product>
        {
            // Electronics
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "MacBook Pro 16-inch",
                Description = "Apple M2 Pro chip, 16GB RAM, 512GB SSD",
                Price = 2499.99m,
                StockQuantity = 50,
                CategoryId = categories[0].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Sony WH-1000XM4",
                Description = "Wireless Noise Cancelling Headphones",
                Price = 349.99m,
                StockQuantity = 100,
                CategoryId = categories[0].Id,
                CreatedAt = DateTime.UtcNow
            },
            
            // Home & Kitchen
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Ninja Foodi 9-in-1 Deluxe XL",
                Description = "Pressure Cooker and Air Fryer",
                Price = 219.99m,
                StockQuantity = 75,
                CategoryId = categories[1].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Dyson V15 Detect",
                Description = "Cordless Vacuum Cleaner with Laser Detection",
                Price = 699.99m,
                StockQuantity = 30,
                CategoryId = categories[1].Id,
                CreatedAt = DateTime.UtcNow
            },

            // Books
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Atomic Habits",
                Description = "An Easy & Proven Way to Build Good Habits by James Clear",
                Price = 24.99m,
                StockQuantity = 200,
                CategoryId = categories[2].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "The Psychology of Money",
                Description = "Timeless lessons on wealth, greed, and happiness by Morgan Housel",
                Price = 19.99m,
                StockQuantity = 150,
                CategoryId = categories[2].Id,
                CreatedAt = DateTime.UtcNow
            },

            // Sports & Outdoors
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Hydroflask 32oz",
                Description = "Wide Mouth Stainless Steel Water Bottle",
                Price = 44.95m,
                StockQuantity = 120,
                CategoryId = categories[3].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Nike Pegasus 39",
                Description = "Running Shoes with React Foam",
                Price = 129.99m,
                StockQuantity = 80,
                CategoryId = categories[3].Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.AddRange(products);
        context.SaveChanges();
    }
}