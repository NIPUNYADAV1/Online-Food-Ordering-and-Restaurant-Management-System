namespace FoodDelivery.Api.DTOs;

public sealed record AuthResponse(int Id, string Name, string Email, string Role, string Token);
