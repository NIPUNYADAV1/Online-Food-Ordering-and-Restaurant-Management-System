namespace FoodDelivery.Api.DTOs;

public sealed record PaymentOrderResponse(int OrderId, string RazorpayOrderId, string KeyId, int Amount, string Currency);
