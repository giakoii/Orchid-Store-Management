using System.Text.Json.Serialization;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Domain.ReadModels;

public class AccountCollection
{
    [JsonPropertyName("id")]
    public int AccountId { get; set; }

    [JsonPropertyName("name")]
    public string AccountName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("role_id")]
    public int RoleId { get; set; }

    [JsonPropertyName("role")]
    public RoleCollection Role { get; set; }

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

    [JsonPropertyName("orders")]
    public List<OrderCollection> Orders { get; set; }

    public static AccountCollection FromWriteModel(Account account, bool includeRelated = false)
    {
        var collection = new AccountCollection
        {
            AccountId = account.AccountId,
            AccountName = account.AcountName,
            Email = account.Email,
            RoleId = account.RoleId,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            CreatedBy = account.CreatedBy,
            UpdatedBy = account.UpdatedBy,
        };

        if (includeRelated)
        {
            if (account.Role != null)
            {
                collection.Role = RoleCollection.FromWriteModel(account.Role);
            }

            if (account.Orders?.Any() == true)
            {
                collection.Orders = account.Orders.Select(o => OrderCollection.FromWriteModel(o, false)).ToList();
            }
        }

        return collection;
    }
}