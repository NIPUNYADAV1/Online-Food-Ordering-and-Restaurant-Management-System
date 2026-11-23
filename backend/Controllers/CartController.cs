using System.Security.Claims;
using FoodDelivery.Api.Data;
using FoodDelivery.Api.DTOs;
using FoodDelivery.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.Customer)]
[Route("api/[controller]")]
public sealed class CartController(FoodDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get()
    {
        var cart = await LoadCartAsync();
        return Ok(ToDto(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> Add(CartItemRequest request)
    {
        var food = await db.FoodItems.Include(item => item.Restaurant)
            .SingleOrDefaultAsync(item => item.Id == request.FoodItemId && item.IsAvailable && item.Restaurant.IsActive);
        if (food is null)
            return NotFound(new { message = "Food item is unavailable." });

        var cart = await GetOrCreateCartAsync();
        if (cart.Items.Any(item => item.FoodItem.RestaurantId != food.RestaurantId))
            return Conflict(new { message = "A cart can contain items from only one restaurant. Clear it before switching restaurants." });

        var existing = cart.Items.SingleOrDefault(item => item.FoodItemId == food.Id);
        if (existing is null)
            cart.Items.Add(new CartItem { FoodItemId = food.Id, FoodItem = food, Quantity = request.Quantity });
        else if (existing.Quantity + request.Quantity > 50)
            return BadRequest(new { message = "A maximum of 50 units per item is allowed." });
        else
            existing.Quantity += request.Quantity;

        cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ToDto(cart));
    }

    [HttpPut("items/{id:int}")]
    public async Task<ActionResult<CartDto>> Update(int id, UpdateCartItemRequest request)
    {
        var item = await db.CartItems.Include(cartItem => cartItem.Cart)
            .SingleOrDefaultAsync(cartItem => cartItem.Id == id && cartItem.Cart.UserId == UserId);
        if (item is null)
            return NotFound(new { message = "Cart item not found." });

        item.Quantity = request.Quantity;
        item.Cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ToDto(await LoadCartAsync()));
    }

    [HttpDelete("items/{id:int}")]
    public async Task<ActionResult<CartDto>> Remove(int id)
    {
        var item = await db.CartItems.Include(cartItem => cartItem.Cart)
            .SingleOrDefaultAsync(cartItem => cartItem.Id == id && cartItem.Cart.UserId == UserId);
        if (item is null)
            return NotFound(new { message = "Cart item not found." });

        db.CartItems.Remove(item);
        item.Cart.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ToDto(await LoadCartAsync()));
    }

    [HttpDelete]
    public async Task<ActionResult<CartDto>> Clear()
    {
        var cart = await LoadCartAsync();
        db.CartItems.RemoveRange(cart.Items);
        await db.SaveChangesAsync();
        return Ok(ToDto(await LoadCartAsync()));
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<Cart> GetOrCreateCartAsync()
    {
        var cart = await db.Carts.Include(item => item.Items).ThenInclude(item => item.FoodItem)
            .SingleOrDefaultAsync(item => item.UserId == UserId);
        if (cart is not null)
            return cart;

        cart = new Cart { UserId = UserId };
        db.Carts.Add(cart);
        await db.SaveChangesAsync();
        return cart;
    }

    private async Task<Cart> LoadCartAsync()
    {
        var cart = await db.Carts.AsNoTracking()
            .Include(item => item.Items).ThenInclude(item => item.FoodItem).ThenInclude(food => food.Restaurant)
            .SingleOrDefaultAsync(item => item.UserId == UserId);
        return cart ?? new Cart { UserId = UserId };
    }

    private static CartDto ToDto(Cart cart)
    {
        var items = cart.Items.Select(item => new CartItemDto(item.Id, item.FoodItemId,
            item.FoodItem.Name, item.FoodItem.Restaurant.Name, item.FoodItem.Price,
            item.Quantity, item.FoodItem.Price * item.Quantity)).ToList();
        return new CartDto(cart.Id, items, items.Sum(item => item.LineTotal));
    }
}
