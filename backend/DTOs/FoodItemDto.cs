namespace FoodDelivery.Api.DTOs;

public sealed record FoodItemDto(int Id, int RestaurantId, string RestaurantName, string Name, string Description, string Category, decimal Price, string? ImageUrl, bool IsAvailable);
