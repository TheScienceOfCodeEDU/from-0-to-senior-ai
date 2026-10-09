namespace FromZeroToSeniorAI.Api.Entities;

public class Order
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public required string Status { get; set; }
    public decimal Total { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}

