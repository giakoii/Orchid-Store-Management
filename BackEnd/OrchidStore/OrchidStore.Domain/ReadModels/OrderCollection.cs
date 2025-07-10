using System.Text.Json.Serialization;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Domain.ReadModels;

public class OrderCollection
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("account_id")]
    public int AccountId { get; set; }

    [JsonPropertyName("account")]
    public AccountCollection Account { get; set; }

    [JsonPropertyName("order_date")]
    public DateTime? OrderDate { get; set; }

    [JsonPropertyName("status")]
    public string OrderStatus { get; set; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

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

    [JsonPropertyName("order_details")]
    public List<OrderDetailCollection> OrderDetails { get; set; }

    public static OrderCollection FromWriteModel(Order order, bool includeRelated = false)
    {
        var collection = new OrderCollection
        {
            Id = order.Id,
            AccountId = order.AccountId,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            TotalAmount = order.TotalAmount,
            IsActive = order.IsActive,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            CreatedBy = order.CreatedBy,
            UpdatedBy = order.UpdatedBy,
        };

        if (includeRelated)
        {
            if (order.Account != null)
            {
                collection.Account = AccountCollection.FromWriteModel(order.Account, false);
            }

            if (order.OrderDetails?.Any() == true)
            {
                collection.OrderDetails = order.OrderDetails.Select(od => OrderDetailCollection.FromWriteModel(od, false)).ToList();
            }
        }

        return collection;
    }
}
