using FoodDelivery.Api.Data;
using FoodDelivery.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RestaurantsController(FoodDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RestaurantDto>>> GetAll([FromQuery] string? search, [FromQuery] string? category)
    {
        var query = db.Restaurants
            .AsNoTracking()
            .Where(restaurant => restaurant.IsActive)
            .Include(restaurant => restaurant.FoodItems.Where(item => item.IsAvailable))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(restaurant =>
                EF.Functions.Like(restaurant.Name, $"%{term}%")
                || EF.Functions.Like(restaurant.Cuisine, $"%{term}%")
                || restaurant.FoodItems.Any(item => item.IsAvailable
                    && EF.Functions.Like(item.Name, $"%{term}%")));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryTerm = category.Trim();
            query = query.Where(restaurant => restaurant.FoodItems.Any(item =>
                item.IsAvailable && EF.Functions.Like(item.Category, $"%{categoryTerm}%")));
        }

        var restaurants = await query.OrderBy(restaurant => restaurant.Name).ToListAsync();
        return Ok(restaurants.Select(restaurant => new RestaurantDto(
            restaurant.Id,
            restaurant.Name,
            restaurant.Description,
            restaurant.Cuisine,
            restaurant.Address,
            restaurant.ImageUrl,
            restaurant.FoodItems
                .Where(item => (string.IsNullOrWhiteSpace(category)
                    || item.Category.Contains(category.Trim(), StringComparison.OrdinalIgnoreCase))
                    && (string.IsNullOrWhiteSpace(search)
                        || restaurant.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)
                        || restaurant.Cuisine.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)
                        || item.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)))
                .Select(item => new FoodItemDto(item.Id, item.RestaurantId, restaurant.Name, item.Name,
                    item.Description, item.Category, item.Price, item.ImageUrl, item.IsAvailable))
                .ToList())).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RestaurantDto>> GetById(int id)
    {
        var restaurant = await db.Restaurants
            .AsNoTracking()
            .Include(item => item.FoodItems.Where(food => food.IsAvailable))
            .SingleOrDefaultAsync(item => item.Id == id && item.IsActive);
        if (restaurant is null)
            return NotFound(new { message = "Restaurant not found." });

        return Ok(new RestaurantDto(restaurant.Id, restaurant.Name, restaurant.Description, restaurant.Cuisine,
            restaurant.Address, restaurant.ImageUrl, restaurant.FoodItems
                .Select(item => new FoodItemDto(item.Id, item.RestaurantId, restaurant.Name, item.Name,
                    item.Description, item.Category, item.Price, item.ImageUrl, item.IsAvailable))
                .ToList()));
    }
}
