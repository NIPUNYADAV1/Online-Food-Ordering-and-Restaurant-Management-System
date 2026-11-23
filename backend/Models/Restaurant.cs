namespace FoodDelivery.Api.Models;

public sealed class Restaurant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = "";
    public required string Cuisine { get; set; }
    public required string Address { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int? OwnerId { get; set; }
    public AppUser? Owner { get; set; }
    public List<FoodItem> FoodItems { get; set; } = [];
}
