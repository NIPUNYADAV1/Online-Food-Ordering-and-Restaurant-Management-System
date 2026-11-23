using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record UpdateCartItemRequest([param: Range(1, 50)] int Quantity);
