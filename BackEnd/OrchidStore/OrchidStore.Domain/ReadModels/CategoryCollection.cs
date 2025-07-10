using System.Text.Json.Serialization;
using OrchidStore.Domain.WriteModels;

namespace OrchidStore.Domain.ReadModels;

public class CategoryCollection
{
    [JsonPropertyName("id")]
    public int CategoryId { get; set; }

    [JsonPropertyName("name")]
    public string CategoryName { get; set; }

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

    [JsonPropertyName("parent_category_id")]
    public int? ParentCategoryId { get; set; }

    [JsonPropertyName("parent_category")]
    public CategoryCollection ParentCategory { get; set; }

    [JsonPropertyName("subcategories")]
    public List<CategoryCollection> SubCategories { get; set; }

    [JsonPropertyName("orchids")]
    public List<OrchidCollection> Orchids { get; set; }

    public static CategoryCollection FromWriteModel(Category category, bool includeRelated = false)
    {
        var collection = new CategoryCollection
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedBy = category.UpdatedBy,
            ParentCategoryId = category.ParentCategoryId
        };

        if (includeRelated)
        {
            if (category.ParentCategory != null)
            {
                collection.ParentCategory = FromWriteModel(category.ParentCategory, false);
            }

            if (category.InverseParentCategory?.Any() == true)
            {
                collection.SubCategories = category.InverseParentCategory.Select(c => FromWriteModel(c, false)).ToList();
            }

            if (category.Orchids?.Any() == true)
            {
                collection.Orchids = category.Orchids.Select(o => OrchidCollection.FromWriteModel(o, false)).ToList();
            }
        }

        return collection;
    }
}
