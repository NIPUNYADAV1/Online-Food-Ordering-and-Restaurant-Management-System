namespace FoodDelivery.Api.DTOs;

public sealed record CartItemDto(int Id, int FoodItemId, string Name, string RestaurantName, decimal UnitPrice, int Quantity, decimal LineTotal);
