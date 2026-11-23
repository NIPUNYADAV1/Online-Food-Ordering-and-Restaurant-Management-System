namespace FoodDelivery.Api.DTOs;

public sealed record RestaurantDto(int Id, string Name, string Description, string Cuisine, string Address, string? ImageUrl, List<FoodItemDto> FoodItems);
