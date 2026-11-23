using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record FoodItemRequest(
    [param: Required, StringLength(120)] string Name,
    [param: StringLength(500)] string Description,
    [param: Required, StringLength(80)] string Category,
    [param: Range(0.01, 100000)] decimal Price,
    [param: Url, StringLength(1000)] string? ImageUrl,
    bool IsAvailable = true);
