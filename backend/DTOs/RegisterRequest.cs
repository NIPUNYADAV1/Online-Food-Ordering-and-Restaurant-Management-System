using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record RegisterRequest(
    [param: Required, StringLength(100, MinimumLength = 2)] string Name,
    [param: Required, EmailAddress, StringLength(320)] string Email,
    [param: Required, MinLength(8), StringLength(100)] string Password);
