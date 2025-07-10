using System.Text.Json.Serialization;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Domain.ReadModels;

public class OrderDetailCollection
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("order_id")]
    public int OrderId { get; set; }

    [JsonPropertyName("orchid_id")]
    public int OrchidId { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal => Price * Quantity;

    [JsonPropertyName("order")]
    public OrderCollection Order { get; set; }

    [JsonPropertyName("orchid")]
    public OrchidCollection Orchid { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonPropertyName("created_by")]
    public string CreatedBy { get; set; } = null!;
    
    [JsonPropertyName("updated_by")]
    public string UpdatedBy { get; set; } = null!;

    public static OrderDetailCollection FromWriteModel(OrderDetail orderDetail, bool includeRelated = false)
    {
        var collection = new OrderDetailCollection
        {
            Id = orderDetail.Id,
            OrderId = orderDetail.OrderId,
            OrchidId = orderDetail.OrchidId,
            Price = orderDetail.Price,
            Quantity = orderDetail.Quantity,
            IsActive = orderDetail.IsActive,
            CreatedAt = orderDetail.CreatedAt,
            UpdatedAt = orderDetail.UpdatedAt,
            CreatedBy = orderDetail.CreatedBy,
            UpdatedBy = orderDetail.UpdatedBy,
        };

        if (includeRelated)
        {
            if (orderDetail.Order != null)
            {
                collection.Order = OrderCollection.FromWriteModel(orderDetail.Order, false);
            }

            if (orderDetail.Orchid != null)
            {
                collection.Orchid = OrchidCollection.FromWriteModel(orderDetail.Orchid, false);
            }
        }

        return collection;
    }
}
