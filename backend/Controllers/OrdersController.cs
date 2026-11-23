using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FoodDelivery.Api.Data;
using FoodDelivery.Api.DTOs;
using FoodDelivery.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class OrdersController(
    FoodDbContext db,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpPost("payment/order")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<PaymentOrderResponse>> CreatePaymentOrder()
    {
        var keyId = configuration["Billing:KeyId"];
        var keySecret = configuration["Billing:KeySecret"];
        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Test-mode billing is not configured. Set Billing__KeyId and Billing__KeySecret." });

        var cart = await db.Carts.AsNoTracking()
            .Include(item => item.Items).ThenInclude(item => item.FoodItem).ThenInclude(food => food.Restaurant)
            .SingleOrDefaultAsync(item => item.UserId == UserId);
        if (cart is null || cart.Items.Count == 0)
            return BadRequest(new { message = "Your cart is empty." });
        if (cart.Items.Any(item => !item.FoodItem.IsAvailable || !item.FoodItem.Restaurant.IsActive))
            return BadRequest(new { message = "The cart contains an item that is no longer available." });

        var total = cart.Items.Sum(item => item.FoodItem.Price * item.Quantity);
        var amount = decimal.ToInt32(decimal.Round(total * 100, 0, MidpointRounding.AwayFromZero));
        if (amount <= 0)
            return BadRequest(new { message = "The cart total must be greater than zero." });

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/orders")
        {
            Content = JsonContent.Create(new
            {
                amount,
                currency = "INR",
                receipt = $"food-{UserId}-{Guid.NewGuid():N}"[..32]
            })
        };
        var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authorization);
        using var response = await httpClientFactory.CreateClient().SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "The payment provider could not create an order. Check the test-mode credentials and try again." });

        var gatewayOrder = await response.Content.ReadFromJsonAsync<RazorpayOrder>();
        if (gatewayOrder is null || string.IsNullOrWhiteSpace(gatewayOrder.Id) || gatewayOrder.Amount != amount)
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "The payment provider returned an invalid order." });

        var order = new FoodOrder
        {
            UserId = UserId,
            Status = "AwaitingPayment",
            PaymentStatus = "Pending",
            TotalAmount = total,
            RazorpayOrderId = gatewayOrder.Id,
            Items = cart.Items.Select(item => new OrderItem
            {
                FoodItemId = item.FoodItemId,
                Name = item.FoodItem.Name,
                UnitPrice = item.FoodItem.Price,
                Quantity = item.Quantity
            }).ToList()
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return Ok(new PaymentOrderResponse(order.Id, gatewayOrder.Id, keyId, amount, "INR"));
    }

    [HttpPost("payment/verify")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<OrderDto>> VerifyPayment(VerifyPaymentRequest request)
    {
        var secret = configuration["Billing:KeySecret"];
        if (string.IsNullOrWhiteSpace(secret))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Test-mode billing is not configured." });

        var order = await db.Orders.Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.RazorpayOrderId == request.RazorpayOrderId && item.UserId == UserId);
        if (order is null)
            return NotFound(new { message = "Payment order not found." });
        if (order.PaymentStatus == "Paid")
            return Conflict(new { message = "This payment has already been verified." });
        if (order.Status != "AwaitingPayment")
            return BadRequest(new { message = "This order is not awaiting payment." });

        var expected = Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes($"{request.RazorpayOrderId}|{request.RazorpayPaymentId}")));
        var supplied = request.RazorpaySignature.Trim().ToUpperInvariant();
        if (supplied.Length != expected.Length
            || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(supplied)))
            return BadRequest(new { message = "Payment signature is invalid." });

        order.RazorpayPaymentId = request.RazorpayPaymentId;
        order.PaymentStatus = "Paid";
        order.Status = "Placed";

        var cart = await db.Carts.Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.UserId == UserId);
        if (cart is not null && cart.Items.Count == order.Items.Count
            && cart.Items.All(cartItem => order.Items.Any(orderItem =>
                orderItem.FoodItemId == cartItem.FoodItemId && orderItem.Quantity == cartItem.Quantity)))
            db.CartItems.RemoveRange(cart.Items);

        await db.SaveChangesAsync();
        return Ok(ToDto(order));
    }

    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<List<OrderDto>>> GetMine()
    {
        var orders = await db.Orders.AsNoTracking().Where(order => order.UserId == UserId)
            .Include(order => order.Items).OrderByDescending(order => order.CreatedAt).ToListAsync();
        return Ok(orders.Select(ToDto).ToList());
    }

    [HttpGet("management")]
    [Authorize(Roles = Roles.Owner + "," + Roles.Admin)]
    public async Task<ActionResult<List<OrderDto>>> GetForManagement()
    {
        var query = db.Orders.AsNoTracking().Include(order => order.Items)
            .ThenInclude(item => item.FoodItem).ThenInclude(food => food.Restaurant).AsQueryable();
        if (!User.IsInRole(Roles.Admin))
            query = query.Where(order => order.Items.Any(item => item.FoodItem.Restaurant.OwnerId == UserId));

        var orders = await query.OrderByDescending(order => order.CreatedAt).ToListAsync();
        return Ok(orders.Select(ToDto).ToList());
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.Owner + "," + Roles.Admin)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var order = await db.Orders.Include(item => item.Items)
            .ThenInclude(item => item.FoodItem).ThenInclude(food => food.Restaurant)
            .SingleOrDefaultAsync(item => item.Id == id);
        if (order is null)
            return NotFound(new { message = "Order not found." });
        if (!User.IsInRole(Roles.Admin)
            && !order.Items.Any(item => item.FoodItem.Restaurant.OwnerId == UserId))
            return Forbid();
        if (order.PaymentStatus != "Paid")
            return BadRequest(new { message = "Only paid orders can be updated." });

        var allowed = order.Status switch
        {
            "Placed" => request.Status is "Confirmed" or "Cancelled",
            "Confirmed" => request.Status is "Preparing" or "Cancelled",
            "Preparing" => request.Status == "Ready",
            "Ready" => request.Status == "OutForDelivery",
            "OutForDelivery" => request.Status == "Delivered",
            _ => false
        };
        if (!allowed)
            return BadRequest(new { message = $"Cannot change order status from {order.Status} to {request.Status}." });

        order.Status = request.Status;
        await db.SaveChangesAsync();
        return Ok(ToDto(order));
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static OrderDto ToDto(FoodOrder order) =>
        new(order.Id, order.Status, order.PaymentStatus, order.TotalAmount, order.CreatedAt,
            order.Items.Select(item => new OrderItemDto(item.FoodItemId, item.Name, item.UnitPrice, item.Quantity)).ToList());

    private sealed record RazorpayOrder(string Id, int Amount);
}
