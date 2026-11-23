using FoodDelivery.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(FoodDbContext db, IConfiguration configuration)
    {
        if (!await db.Restaurants.AnyAsync())
        {
            var restaurant = new Restaurant
            {
                Name = "Green Table",
                Description = "Fresh, comforting meals made daily.",
                Cuisine = "Healthy",
                Address = "Downtown",
                FoodItems =
                [
                    new FoodItem
                    {
                        Name = "Avocado Grain Bowl",
                        Description = "Quinoa, greens, and avocado.",
                        Category = "Bowls",
                        Price = 12.50m,
                        ImageUrl = "https://www.realsimple.com/thmb/eR17CQQYSZWKCQ2uAr83IuKIqAk=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/mixed-grains-bowl-beet-ginger-dressing-recipe-0221foo-38c08b070c384c3d850bcdad825220af.jpg"
                    },
                    new FoodItem
                    {
                        Name = "Roasted Tomato Soup",
                        Description = "Slow-roasted tomatoes with herbs.",
                        Category = "Soups",
                        Price = 7.25m,
                        ImageUrl = "https://www.gimmesomeoven.com/wp-content/uploads/2021/09/Roasted-Tomato-Soup-Recipe-8.jpg"
                    },
                    new FoodItem
                    {
                        Name = "Chocolate Oat Cookie",
                        Description = "Baked in-house with dark chocolate.",
                        Category = "Desserts",
                        Price = 4.00m,
                        ImageUrl = "https://www.kitchentreaty.com/wp-content/uploads/2013/12/double-chocolate-chip-oatmeal-cookies-20.jpg"
                    }
                ]
            };
            db.Restaurants.Add(restaurant);
        }

        var sampleImages = new Dictionary<string, string>
        {
            ["Avocado Grain Bowl"] = "https://www.realsimple.com/thmb/eR17CQQYSZWKCQ2uAr83IuKIqAk=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/mixed-grains-bowl-beet-ginger-dressing-recipe-0221foo-38c08b070c384c3d850bcdad825220af.jpg",
            ["Roasted Tomato Soup"] = "https://www.gimmesomeoven.com/wp-content/uploads/2021/09/Roasted-Tomato-Soup-Recipe-8.jpg",
            ["Chocolate Oat Cookie"] = "https://www.kitchentreaty.com/wp-content/uploads/2013/12/double-chocolate-chip-oatmeal-cookies-20.jpg"
        };
        var existingSampleItems = await db.FoodItems
            .Where(item => sampleImages.Keys.Contains(item.Name))
            .ToListAsync();
        foreach (var item in existingSampleItems)
            item.ImageUrl = sampleImages[item.Name];

        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword)
            && !await db.Users.AnyAsync(user => user.Email == adminEmail.ToLower()))
        {
            db.Users.Add(new AppUser
            {
                Name = "Administrator",
                Email = adminEmail.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = Roles.Admin
            });
        }

        await db.SaveChangesAsync();
    }
}
