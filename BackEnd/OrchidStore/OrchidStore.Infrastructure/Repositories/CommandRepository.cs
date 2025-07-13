using System.Linq.Expressions;
using Marten;
using Microsoft.EntityFrameworkCore;
using OrchidStore.Application.Repositories;
using OrchidStore.Infrastructure.Data.Helpers;

namespace OrchidStore.Infrastructure.Repositories;

public class CommandRepository<TEntity>(AppDbContext context, IDocumentSession session) : ICommandRepository<TEntity> where TEntity : class
{
    private DbSet<TEntity> DbSet => context.Set<TEntity>();
    
    /// <summary>
    /// Find records/ record
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="isTracking"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    public IQueryable<TEntity?> Find(Expression<Func<TEntity, bool>>? predicate = null, bool isTracking = true, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = DbSet;

        if (predicate != null) query = query.Where(predicate);

        query = includes.Aggregate(query, (current, inc) => current.Include(inc));

        if (!isTracking) query = query.AsNoTracking();

        return query;
    }

    /// <summary>
    /// Add entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool Add(TEntity entity)
    {
        context.Add(entity);
        return true;
    }

    /// <summary>
    /// Add entity asynchronously
    /// </summary>
    public async Task AddAsync(TEntity entity)
    {
        await context.AddAsync(entity);
    }

    /// <summary>
    /// Add a range of entities
    /// </summary>
    /// <param name="entities"></param>
    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
       await context.AddRangeAsync(entities);
    }
    
    /// <summary>
    /// Add entity to the Marten session
    /// </summary>
    public void Store<TCollection>(TCollection entity, string updatedBy, bool isModified = false, bool needLogicalDelete = false) where TCollection : class
    {
        EntityMetadataHelper.SetCommonValuesForMarten(new List<object> { entity }, updatedBy, isModified, needLogicalDelete);
        session.Store(entity);
    }

    /// <summary>
    /// Add a range of entities
    /// </summary>
    /// <param name="entities"></param>
    public void StoreRange<TCollection>(List<TCollection> entities, string updatedBy, bool isModified = false, bool needLogicalDelete = false) where TCollection : class
    {
        EntityMetadataHelper.SetCommonValuesForMarten(entities.Cast<object>().ToList(), updatedBy, isModified, needLogicalDelete);
        foreach (var entity in entities)
        {
            session.Store(entity);
        }
    }

    /// <summary>
    /// Update entity
    /// </summary>
    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    /// <summary>
    /// Save changes
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="needLogicalDelete"></param>
    /// <exception cref="NotImplementedException"></exception>
    public int SaveChanges(string userName, bool needLogicalDelete = false)
    {
        return context.SaveChanges(userName, needLogicalDelete);
    }

    /// <summary>
    /// Save changes asynchronously
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="needLogicalDelete"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<int> SaveChangesAsync(string userName, bool needLogicalDelete = false)
    {
        return await context.SaveChangesAsync(userName, needLogicalDelete);
    }
    
    public async Task SessionSavechanges()
    {
        await session.SaveChangesAsync();
    }

    /// <summary>
    /// Execute multiple operations within a transaction.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public void ExecuteInTransaction(Func<bool> action)
    {
        // Begin transaction
        using var transaction = context.Database.BeginTransaction();
        try
        {
            // Execute action
            if (action())
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Execute multiple operations within a transaction asynchronously.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task ExecuteInTransactionAsync(Func<Task<bool>> action)
    {
        // Begin transaction
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Execute action
            if (await action())
            {
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}