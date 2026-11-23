namespace FoodDelivery.Api.DTOs;

public sealed record OrderItemDto(int FoodItemId, string Name, decimal UnitPrice, int Quantity);
