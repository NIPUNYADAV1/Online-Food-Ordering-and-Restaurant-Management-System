using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record UpdateOrderStatusRequest(
    [param: Required, RegularExpression("^(Confirmed|Preparing|Ready|OutForDelivery|Delivered|Cancelled)$")] string Status);
