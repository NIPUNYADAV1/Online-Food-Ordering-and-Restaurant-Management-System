using FoodDelivery.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Api.Data;

public sealed class FoodDbContext(DbContextOptions<FoodDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<FoodOrder> Orders => Set<FoodOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>().ToTable("Cart");
        modelBuilder.Entity<AppUser>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<AppUser>().Property(user => user.Email).HasMaxLength(320);
        modelBuilder.Entity<FoodItem>().Property(item => item.Price).HasPrecision(10, 2);
        modelBuilder.Entity<FoodOrder>().Property(order => order.TotalAmount).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(item => item.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<Cart>().HasIndex(cart => cart.UserId).IsUnique();
        modelBuilder.Entity<CartItem>().HasIndex(item => new { item.CartId, item.FoodItemId }).IsUnique();
        modelBuilder.Entity<FoodOrder>().HasIndex(order => order.RazorpayOrderId).IsUnique();
        modelBuilder.Entity<Restaurant>().HasIndex(restaurant => restaurant.OwnerId).IsUnique();
        modelBuilder.Entity<Restaurant>()
            .HasOne(restaurant => restaurant.Owner)
            .WithMany()
            .HasForeignKey(restaurant => restaurant.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.FoodItem)
            .WithMany()
            .HasForeignKey(item => item.FoodItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
