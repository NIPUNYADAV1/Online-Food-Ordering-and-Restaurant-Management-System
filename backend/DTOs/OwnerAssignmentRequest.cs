using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record OwnerAssignmentRequest([param: Range(1, int.MaxValue)] int? OwnerId);
