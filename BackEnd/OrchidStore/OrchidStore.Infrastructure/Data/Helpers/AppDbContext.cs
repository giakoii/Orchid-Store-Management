using Microsoft.EntityFrameworkCore;
using OrchidStore.Infrastructure.Data.Contexts;

namespace OrchidStore.Infrastructure.Data.Helpers;

public class AppDbContext : OrchidStoreContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(new DbContextOptions<OrchidStoreContext>())
    {
    }
    
    /// <summary>
    /// Save changes async with common value
    /// </summary>
    /// <param name="updateUserId"></param>
    /// <param name="needLogicalDelete"></param>
    /// <returns></returns>
    public Task<int> SaveChangesAsync(string updateUserId, bool needLogicalDelete = false)
    {
        this.SetCommonValue(updateUserId, needLogicalDelete);
        return base.SaveChangesAsync();
    }
    
    /// <summary>
    /// Save changes with common value
    /// </summary>
    /// <param name="updateUserId"></param>
    /// <param name="needLogicalDelete"></param>
    /// <returns></returns>
    public int SaveChanges(string updateUserId, bool needLogicalDelete = false)
    {
        this.SetCommonValue(updateUserId, needLogicalDelete);
        return base.SaveChanges();
    }
    
    /// <summary>
    /// Set common value for all Entities
    /// </summary>
    /// <param name="updateUser"></param>
    /// <param name="needLogicalDelete"></param>
    private void SetCommonValue(string updateUser, bool needLogicalDelete = false)
    {
        // Register
        var newEntities = this.ChangeTracker.Entries()
            .Where(
                x => x.State == EntityState.Added &&
                x.Entity != null
                )
            .Select(e => e.Entity);

        // Update
        var modifiedEntities = this.ChangeTracker.Entries()
            .Where(
                x => x.State == EntityState.Modified &&
                    x.Entity != null
                    )
                .Select(e => e.Entity);

        // Get current time
        var now = DateTime.UtcNow;
        // Add
        foreach (dynamic newEntity in newEntities)
        {
            try
            {
                newEntity.IsActive = true;
                newEntity.CreatedAt = now;
                newEntity.CreatedBy = updateUser;
                newEntity.UpdatedBy = updateUser;
                newEntity.UpdatedAt = now;
            }
            catch (IOException e)
            {
                
            }
        }

        // Set modifiedEntities
        foreach (dynamic modifiedEntity in modifiedEntities)
        {
            try
            {
                if (needLogicalDelete)
                {
                    // Delete
                    modifiedEntity.IsActive = false;
                    modifiedEntity.UpdatedBy = updateUser;
                }
                else
                {
                    // Normal
                    modifiedEntity.IsActive = true;
                    modifiedEntity.UpdatedBy = updateUser;
                }
                modifiedEntity.UpdatedAt = now;
            }
            catch (IOException e)
            {
            }
        }

    }
}