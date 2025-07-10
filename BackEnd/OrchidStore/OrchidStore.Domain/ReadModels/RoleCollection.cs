using System.Text.Json.Serialization;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Domain.ReadModels;

public class RoleCollection
{
    [JsonPropertyName("id")]
    public int RoleId { get; set; }

    [JsonPropertyName("name")]
    public string RoleName { get; set; }

    [JsonPropertyName("accounts")]
    public List<AccountCollection> Accounts { get; set; }

    public static RoleCollection FromWriteModel(Role role, bool includeRelated = false)
    {
        var collection = new RoleCollection
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName
        };

        if (includeRelated && role.Accounts?.Any() == true)
        {
            collection.Accounts = role.Accounts.Select(a => AccountCollection.FromWriteModel(a, false)).ToList();
        }

        return collection;
    }
}
