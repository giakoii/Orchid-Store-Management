using System.Text.Json.Serialization;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Domain.ReadModels;

public class OrchidCollection
{
    [JsonPropertyName("id")]
    public int OrchidId { get; set; }

    [JsonPropertyName("name")]
    public string OrchidName { get; set; }

    [JsonPropertyName("description")]
    public string OrchidDescription { get; set; }

    [JsonPropertyName("image_url")]
    public string OrchidUrl { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("is_natural")]
    public bool IsNatural { get; set; }

    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }

    [JsonPropertyName("category")]
    public CategoryCollection Category { get; set; }

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

    public static OrchidCollection FromWriteModel(Orchid orchid, bool includeRelated = false)
    {
        var collection = new OrchidCollection
        {
            OrchidId = orchid.OrchidId,
            OrchidName = orchid.OrchidName,
            OrchidDescription = orchid.OrchidDescription,
            OrchidUrl = orchid.OrchidUrl,
            Price = orchid.Price,
            IsNatural = orchid.IsNatural,
            CategoryId = orchid.CategoryId,
            IsActive = orchid.IsActive,
            CreatedAt = orchid.CreatedAt,
            UpdatedAt = orchid.UpdatedAt,
            CreatedBy = orchid.CreatedBy,
            UpdatedBy = orchid.UpdatedBy,
        };

        if (includeRelated)
        {
            if (orchid.Category != null)
            {
                collection.Category = CategoryCollection.FromWriteModel(orchid.Category, false);
            }

            if (orchid.OrderDetails?.Any() == true)
            {
                collection.OrderDetails = orchid.OrderDetails.Select(od => OrderDetailCollection.FromWriteModel(od, false)).ToList();
            }
        }

        return collection;
    }
}
