using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record LoginRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password);
