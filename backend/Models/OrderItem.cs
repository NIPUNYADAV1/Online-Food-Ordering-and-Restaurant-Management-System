namespace FoodDelivery.Api.Models;

public sealed class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public FoodOrder Order { get; set; } = null!;
    public int FoodItemId { get; set; }
    public FoodItem FoodItem { get; set; } = null!;
    public required string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
