using FoodDelivery.Api.Data;
using FoodDelivery.Api.DTOs;
using FoodDelivery.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin")]
public sealed class AdminController(FoodDbContext db) : ControllerBase
{
    [HttpPost("owners")]
    public async Task<ActionResult> CreateOwner(OwnerRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existingUser = await db.Users.SingleOrDefaultAsync(user => user.Email == email);
        if (existingUser is not null)
        {
            if (existingUser.Role != Roles.Customer)
                return Conflict(new
                {
                    message = existingUser.Role == Roles.Owner
                        ? "This account is already a restaurant owner. Assign it to a restaurant below."
                        : "This email belongs to an administrator and cannot be used for a restaurant owner."
                });

            existingUser.Name = request.Name.Trim();
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            existingUser.Role = Roles.Owner;
            await db.SaveChangesAsync();
            return Ok(new
            {
                existingUser.Id,
                existingUser.Name,
                existingUser.Email,
                existingUser.Role,
                wasPromoted = true,
                message = "Existing customer account promoted to restaurant owner. They can sign in with the temporary password you set."
            });
        }

        var owner = new AppUser
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Roles.Owner
        };
        db.Users.Add(owner);
        await db.SaveChangesAsync();
        return Created($"/api/admin/owners/{owner.Id}", new
        {
            owner.Id,
            owner.Name,
            owner.Email,
            owner.Role,
            wasPromoted = false,
            message = "Restaurant owner account created."
        });
    }

    [HttpGet("owners")]
    public async Task<ActionResult> GetOwners() =>
        Ok(await db.Users.AsNoTracking().Where(user => user.Role == Roles.Owner)
            .OrderBy(user => user.Name)
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.Email,
                Assignment = db.Restaurants
                    .Where(restaurant => restaurant.OwnerId == user.Id)
                    .Select(restaurant => new
                    {
                        RestaurantId = (int?)restaurant.Id,
                        RestaurantName = restaurant.Name
                    })
                    .FirstOrDefault()
            }).ToListAsync());

    [HttpPost("restaurants")]
    public async Task<ActionResult<RestaurantDto>> CreateRestaurant(RestaurantRequest request)
    {
        var restaurant = new Restaurant
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Cuisine = request.Cuisine.Trim(),
            Address = request.Address.Trim(),
            ImageUrl = request.ImageUrl
        };
        db.Restaurants.Add(restaurant);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(RestaurantsController.GetById), "Restaurants", new { id = restaurant.Id },
            new RestaurantDto(restaurant.Id, restaurant.Name, restaurant.Description, restaurant.Cuisine,
                restaurant.Address, restaurant.ImageUrl, []));
    }

    [HttpPut("restaurants/{id:int}")]
    public async Task<IActionResult> UpdateRestaurant(int id, RestaurantRequest request)
    {
        var restaurant = await db.Restaurants.FindAsync(id);
        if (restaurant is null)
            return NotFound(new { message = "Restaurant not found." });

        restaurant.Name = request.Name.Trim();
        restaurant.Description = request.Description.Trim();
        restaurant.Cuisine = request.Cuisine.Trim();
        restaurant.Address = request.Address.Trim();
        restaurant.ImageUrl = request.ImageUrl;
        restaurant.IsActive = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("restaurants/{id:int}")]
    public async Task<IActionResult> DeactivateRestaurant(int id)
    {
        var restaurant = await db.Restaurants.FindAsync(id);
        if (restaurant is null)
            return NotFound(new { message = "Restaurant not found." });

        restaurant.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("restaurants/{id:int}/owner")]
    public async Task<IActionResult> AssignOwner(int id, OwnerAssignmentRequest request)
    {
        var restaurant = await db.Restaurants.FindAsync(id);
        if (restaurant is null)
            return NotFound(new { message = "Restaurant not found." });

        if (request.OwnerId is null)
        {
            restaurant.OwnerId = null;
            await db.SaveChangesAsync();
            return NoContent();
        }

        var owner = await db.Users.SingleOrDefaultAsync(user => user.Id == request.OwnerId && user.Role == Roles.Owner);
        if (owner is null)
            return BadRequest(new { message = "Choose an existing restaurant owner." });

        var existingAssignment = await db.Restaurants
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.OwnerId == owner.Id && item.Id != restaurant.Id);
        if (existingAssignment is not null)
            return Conflict(new
            {
                message = $"{owner.Name} is already assigned to {existingAssignment.Name}. Each owner can manage only one restaurant."
            });

        restaurant.OwnerId = owner.Id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("restaurants/{id:int}/food-items")]
    public async Task<ActionResult<FoodItemDto>> CreateFoodItem(int id, FoodItemRequest request)
    {
        var restaurant = await db.Restaurants.SingleOrDefaultAsync(item => item.Id == id && item.IsActive);
        if (restaurant is null)
            return NotFound(new { message = "Restaurant not found." });

        var food = new FoodItem
        {
            RestaurantId = id,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            IsAvailable = request.IsAvailable
        };
        db.FoodItems.Add(food);
        await db.SaveChangesAsync();
        return Created($"/api/restaurants/{id}", ToDto(food, restaurant.Name));
    }

    [HttpPut("food-items/{id:int}")]
    public async Task<IActionResult> UpdateFoodItem(int id, FoodItemRequest request)
    {
        var food = await db.FoodItems.FindAsync(id);
        if (food is null)
            return NotFound(new { message = "Food item not found." });

        food.Name = request.Name.Trim();
        food.Description = request.Description.Trim();
        food.Category = request.Category.Trim();
        food.Price = request.Price;
        food.ImageUrl = request.ImageUrl;
        food.IsAvailable = request.IsAvailable;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("food-items/{id:int}")]
    public async Task<IActionResult> RemoveFoodItem(int id)
    {
        var food = await db.FoodItems.FindAsync(id);
        if (food is null)
            return NotFound(new { message = "Food item not found." });

        food.IsAvailable = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static FoodItemDto ToDto(FoodItem food, string restaurantName) =>
        new(food.Id, food.RestaurantId, restaurantName, food.Name, food.Description,
            food.Category, food.Price, food.ImageUrl, food.IsAvailable);
}
