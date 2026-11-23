using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Api.DTOs;

public sealed record VerifyPaymentRequest(
    [param: Required] string RazorpayOrderId,
    [param: Required] string RazorpayPaymentId,
    [param: Required] string RazorpaySignature);
