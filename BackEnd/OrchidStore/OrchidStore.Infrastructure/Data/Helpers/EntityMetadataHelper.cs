namespace OrchidStore.Infrastructure.Data.Helpers;

public static class EntityMetadataHelper
{
    /// <summary>
    /// Set common values for entities in Marten
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="updatedBy"></param>
    /// <param name="isModified"></param>
    /// <param name="needLogicalDelete"></param>
    public static void SetCommonValuesForMarten(IList<object> entities, string updatedBy, bool isModified, bool needLogicalDelete = false)
    {
        var now = DateTime.UtcNow;

        foreach (var entity in entities)
        {
            var type = entity.GetType();

            if (!isModified)
            {
                TrySetProperty(type, entity, "IsActive", true);
            }
            else // Modified
            {
                if (needLogicalDelete)
                {
                    TrySetProperty(type, entity, "IsActive", false);
                }
                else
                {
                    TrySetProperty(type, entity, "IsActive", true);
                }
            }

            TrySetProperty(type, entity, "CreatedAt", now);
            TrySetProperty(type, entity, "CreatedBy", updatedBy);
            TrySetProperty(type, entity, "UpdatedAt", now);
            TrySetProperty(type, entity, "UpdatedBy", updatedBy);
        }
    }

    private static void TrySetProperty(Type type, object entity, string propertyName, object value)
    {
        var property = type.GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, value);
        }
    }
}