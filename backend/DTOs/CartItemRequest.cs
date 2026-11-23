using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record CartItemRequest([param: Range(1, int.MaxValue)] int FoodItemId, [param: Range(1, 50)] int Quantity);
