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
        var products = new List<Product>();
        
        // Add existing products
        products.AddRange(new[]
        {
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
        });

        // Electronics Category
        products.AddRange(new[]
        {
            CreateProduct("iPhone 15 Pro Max", "256GB, Titanium, A17 Pro chip", 1199.99m, 100, categories[0]),
            CreateProduct("Samsung Galaxy S24 Ultra", "512GB, AI-powered camera system", 1299.99m, 85, categories[0]),
            CreateProduct("iPad Air 5th Gen", "M1 chip, 10.9-inch Liquid Retina", 599.99m, 120, categories[0]),
            CreateProduct("Dell XPS 15", "12th Gen i9, 32GB RAM, RTX 3050 Ti", 2199.99m, 40, categories[0]),
            CreateProduct("LG C3 OLED TV 65\"", "4K Smart OLED evo TV with AI Processing", 1999.99m, 30, categories[0]),
            CreateProduct("Sony A7 IV", "Full-frame Mirrorless Camera", 2499.99m, 25, categories[0]),
            CreateProduct("Bose QuietComfort Ultra", "Premium noise cancelling headphones", 379.99m, 150, categories[0]),
            CreateProduct("ASUS ROG Swift", "27\" 360Hz Gaming Monitor", 699.99m, 45, categories[0]),
            CreateProduct("Steam Deck OLED", "512GB Gaming Handheld", 549.99m, 60, categories[0]),
            CreateProduct("DJI Mini 3 Pro", "Lightweight 4K Drone", 759.99m, 35, categories[0]),
            CreateProduct("Garmin Fenix 7", "Premium Multisport GPS Watch", 699.99m, 80, categories[0]),
            CreateProduct("Canon R6 Mark II", "Mirrorless Digital Camera", 2499.99m, 20, categories[0]),
            CreateProduct("Sonos Arc", "Premium Smart Soundbar", 899.99m, 40, categories[0]),
            CreateProduct("Razer Blade 16", "Gaming Laptop RTX 4090", 3499.99m, 15, categories[0]),
            CreateProduct("Google Pixel 8 Pro", "256GB, AI-powered camera", 999.99m, 90, categories[0]),
            CreateProduct("Apple Watch Ultra 2", "49mm Titanium Case", 799.99m, 75, categories[0]),
            CreateProduct("Samsung Odyssey G9", "49\" Ultra-wide Gaming Monitor", 1299.99m, 25, categories[0]),
            CreateProduct("Sony WF-1000XM5", "Wireless Earbuds", 299.99m, 120, categories[0]),
            CreateProduct("Meta Quest 3", "256GB VR Headset", 499.99m, 50, categories[0]),
            CreateProduct("ASUS ROG Strix", "Gaming PC RTX 4080", 2999.99m, 20, categories[0]),
            CreateProduct("iPhone 15", "128GB Base Model", 799.99m, 150, categories[0]),
            CreateProduct("MacBook Air M2", "13.6\" 8GB RAM 256GB", 1099.99m, 85, categories[0]),
            CreateProduct("AirPods Pro 2", "with USB-C", 249.99m, 200, categories[0]),
            CreateProduct("iPad Pro 12.9\"", "M2 chip, 256GB", 1099.99m, 70, categories[0])
        });

        // Home & Kitchen Category
        products.AddRange(new[]
        {
            CreateProduct("KitchenAid Pro Stand Mixer", "5-Quart Professional Series", 449.99m, 60, categories[1]),
            CreateProduct("Breville Barista Express", "Espresso Machine", 699.99m, 40, categories[1]),
            CreateProduct("Vitamix A3500", "Smart Blender System", 649.99m, 45, categories[1]),
            CreateProduct("Samsung Smart Fridge", "Family Hub, 26.5 cu ft", 3299.99m, 15, categories[1]),
            CreateProduct("Instant Pot Pro Plus", "10-in-1 Pressure Cooker", 169.99m, 100, categories[1]),
            CreateProduct("Dyson V12 Detect Slim", "Cordless Vacuum", 649.99m, 55, categories[1]),
            CreateProduct("iRobot Roomba j9+", "Self-emptying Robot Vacuum", 899.99m, 40, categories[1]),
            CreateProduct("Le Creuset Dutch Oven", "5.5Qt Enameled Cast Iron", 399.99m, 70, categories[1]),
            CreateProduct("Philips Air Fryer XXL", "Digital Display", 249.99m, 85, categories[1]),
            CreateProduct("Miele Complete C3", "Canister Vacuum", 899.99m, 30, categories[1]),
            CreateProduct("Zojirushi Rice Cooker", "Neuro Fuzzy Logic", 199.99m, 65, categories[1]),
            CreateProduct("Nespresso VertuoPlus", "Coffee & Espresso Maker", 199.99m, 90, categories[1]),
            CreateProduct("Weber Genesis E-325s", "Smart Gas Grill", 1099.99m, 25, categories[1]),
            CreateProduct("Bosch 800 Series", "Dishwasher", 1299.99m, 30, categories[1]),
            CreateProduct("Staub Cast Iron Skillet", "12-inch Traditional", 199.99m, 80, categories[1]),
            CreateProduct("Technivorm Moccamaster", "Coffee Maker", 349.99m, 45, categories[1]),
            CreateProduct("Samsung Bespoke", "Smart Microwave", 499.99m, 50, categories[1]),
            CreateProduct("Coway Air Purifier", "True HEPA with Air Quality", 299.99m, 70, categories[1]),
            CreateProduct("GE Profile Smart Oven", "Built-in Convection", 2999.99m, 20, categories[1]),
            CreateProduct("Ninja Speedi", "Rapid Cooker & Air Fryer", 199.99m, 95, categories[1]),
            CreateProduct("LG Front Load Washer", "Smart Wi-Fi Enabled", 999.99m, 35, categories[1]),
            CreateProduct("SodaStream Art", "Sparkling Water Maker", 129.99m, 110, categories[1]),
            CreateProduct("Whirlpool Side-by-Side", "26 cu. ft. Refrigerator", 1599.99m, 25, categories[1]),
            CreateProduct("Cuisinart Food Processor", "14-Cup Custom", 229.99m, 75, categories[1])
        });

        // Books Category
        products.AddRange(new[]
        {
            CreateProduct("Tomorrow, and Tomorrow", "2024 Best Seller by Gabrielle Zevin", 24.99m, 150, categories[2]),
            CreateProduct("Iron Flame", "Fourth Wing Series by Rebecca Yarros", 19.99m, 200, categories[2]),
            CreateProduct("The Woman in Me", "Memoir by Britney Spears", 29.99m, 175, categories[2]),
            CreateProduct("House of Flame and Shadow", "Crescent City #3", 27.99m, 160, categories[2]),
            CreateProduct("A Court of Thorns and Roses Box Set", "Sarah J. Maas Collection", 89.99m, 50, categories[2]),
            CreateProduct("The Creative Act", "By Rick Rubin", 32.99m, 120, categories[2]),
            CreateProduct("Fourth Wing", "Rebecca Yarros Bestseller", 19.99m, 180, categories[2]),
            CreateProduct("Democracy Awakening", "Heather Cox Richardson", 28.99m, 140, categories[2]),
            CreateProduct("The Boys in the Boat", "Daniel James Brown", 17.99m, 160, categories[2]),
            CreateProduct("Build the Life You Want", "Brooks and Winfrey", 24.99m, 200, categories[2]),
            CreateProduct("Harry Potter Box Set", "Complete Series", 159.99m, 45, categories[2]),
            CreateProduct("Dune Deluxe Edition", "Frank Herbert Classic", 34.99m, 85, categories[2]),
            CreateProduct("Project 1619", "Nikole Hannah-Jones", 27.99m, 120, categories[2]),
            CreateProduct("The Light We Carry", "Michelle Obama", 29.99m, 150, categories[2]),
            CreateProduct("Meditations", "Marcus Aurelius (Hardcover)", 22.99m, 100, categories[2]),
            CreateProduct("Rich Dad Poor Dad", "Robert Kiyosaki", 16.99m, 175, categories[2]),
            CreateProduct("Think and Grow Rich", "Napoleon Hill", 15.99m, 190, categories[2]),
            CreateProduct("Lord of the Rings Set", "Illustrated Edition", 129.99m, 40, categories[2]),
            CreateProduct("Foundation Trilogy", "Isaac Asimov", 49.99m, 70, categories[2]),
            CreateProduct("1984", "George Orwell (Annotated)", 19.99m, 150, categories[2]),
            CreateProduct("Deep Work", "Cal Newport", 26.99m, 130, categories[2]),
            CreateProduct("Zero to One", "Peter Thiel", 24.99m, 145, categories[2]),
            CreateProduct("Clean Code", "Robert C. Martin", 44.99m, 80, categories[2]),
            CreateProduct("The Pragmatic Programmer", "Hunt & Thomas", 49.99m, 75, categories[2])
        });

        // Sports & Outdoors Category
        products.AddRange(new[]
        {
            CreateProduct("Yeti Tundra 65", "Premium Cooler", 399.99m, 40, categories[3]),
            CreateProduct("Garmin Edge 840", "GPS Bike Computer", 449.99m, 55, categories[3]),
            CreateProduct("Trek Fuel EX 8", "Full Suspension Mountain Bike", 3499.99m, 15, categories[3]),
            CreateProduct("Osprey Atmos AG 65", "Hiking Backpack", 299.99m, 70, categories[3]),
            CreateProduct("Patagonia Down Sweater", "Men's Hoody", 279.99m, 85, categories[3]),
            CreateProduct("Brooks Ghost 15", "Running Shoes", 139.99m, 120, categories[3]),
            CreateProduct("The North Face Tent", "2-Person Backpacking", 349.99m, 45, categories[3]),
            CreateProduct("Callaway Paradym Driver", "Golf Club", 599.99m, 30, categories[3]),
            CreateProduct("REI Co-op Magma", "0-Degree Sleeping Bag", 389.99m, 50, categories[3]),
            CreateProduct("Manduka PRO Yoga Mat", "6mm Premium Mat", 129.99m, 90, categories[3]),
            CreateProduct("Traeger Ironwood 885", "WiFi Pellet Grill", 1499.99m, 25, categories[3]),
            CreateProduct("DJI Action 4", "Action Camera", 399.99m, 60, categories[3]),
            CreateProduct("Specialized S-Works", "Carbon Road Bike", 7999.99m, 10, categories[3]),
            CreateProduct("Black Diamond Harness", "Climbing Gear", 79.99m, 65, categories[3]),
            CreateProduct("Wilson Pro Staff RF97", "Tennis Racket", 269.99m, 40, categories[3]),
            CreateProduct("Coleman Camping Set", "4-Person Bundle", 299.99m, 55, categories[3]),
            CreateProduct("Arc'teryx Beta AR", "Gore-Tex Jacket", 599.99m, 35, categories[3]),
            CreateProduct("Thule Roof Box", "Car Storage 16 cu ft", 749.99m, 25, categories[3]),
            CreateProduct("NordicTrack S22i", "Studio Cycle", 1999.99m, 20, categories[3]),
            CreateProduct("Salomon Quest 4", "Hiking Boots", 229.99m, 75, categories[3]),
            CreateProduct("Hobie Mirage Kayak", "Pedal Drive System", 2899.99m, 15, categories[3]),
            CreateProduct("RTIC Soft Cooler", "30-Can Portable", 199.99m, 85, categories[3]),
            CreateProduct("Shimano Ultegra", "Road Bike Groupset", 1299.99m, 30, categories[3]),
            CreateProduct("GoPro Hero 12", "Black Bundle", 449.99m, 55, categories[3])
        });

        context.AddRange(products);
        context.SaveChanges();
    }

    private static Product CreateProduct(string name, string description, decimal price, int stockQuantity, Category category)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };
    }
}