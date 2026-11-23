namespace FoodDelivery.Api.DTOs;

public sealed record CartDto(int Id, List<CartItemDto> Items, decimal Total);
