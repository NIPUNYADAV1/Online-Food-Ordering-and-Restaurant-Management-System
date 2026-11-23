using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record RestaurantRequest(
    [param: Required, StringLength(120)] string Name,
    [param: StringLength(500)] string Description,
    [param: Required, StringLength(80)] string Cuisine,
    [param: Required, StringLength(300)] string Address,
    [param: Url, StringLength(1000)] string? ImageUrl);
