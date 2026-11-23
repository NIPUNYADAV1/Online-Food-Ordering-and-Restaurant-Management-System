namespace FoodDelivery.Api.Models;

public sealed class FoodOrder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public required string Status { get; set; }
    public required string PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = [];
}
