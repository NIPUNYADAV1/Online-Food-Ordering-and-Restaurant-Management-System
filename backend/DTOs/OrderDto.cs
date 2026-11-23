namespace FoodDelivery.Api.DTOs;

public sealed record OrderDto(int Id, string Status, string PaymentStatus, decimal TotalAmount, DateTime CreatedAt, List<OrderItemDto> Items);
