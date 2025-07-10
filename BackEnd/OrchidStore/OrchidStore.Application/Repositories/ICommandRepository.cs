using System.Linq.Expressions;

namespace OrchidStore.Application.Repositories;

public interface ICommandRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Add entity to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    bool Add(TEntity entity);
    
    /// <summary>
    /// Add entity to the database asynchronously.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task AddAsync(TEntity entity);
    
    /// <summary>
    /// Add a range of entities to the database asynchronously.
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Store a collection of entities in the marten.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updatedBy"></param>
    /// <param name="isModified"></param>
    /// <typeparam name="TCollection"></typeparam>
    void Store<TCollection>(TCollection entity, string updatedBy, bool isModified = false) where TCollection : class;

    /// <summary>
    /// Store a range of entities in the marten.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="updatedBy"></param>
    /// <param name="isModified"></param>
    /// <typeparam name="TCollection"></typeparam>
    void StoreRange<TCollection>(List<TCollection> entities, string updatedBy, bool isModified = false) where TCollection : class;
    
    /// <summary>
    /// Execute a function within a transaction.
    /// </summary>
    /// <param name="action"></param>
    void ExecuteInTransaction(Func<bool> action);
    
    /// <summary>
    /// Execute a function within a transaction asynchronously.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task ExecuteInTransactionAsync(Func<Task<bool>> action);
    
    /// <summary>
    /// Get records/ record based on a predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="isTracking"></param>
    /// <param name="includes"></param>
    /// <returns></returns>
    IQueryable<TEntity?> Find(Expression<Func<TEntity, bool>>? predicate = null!, bool isTracking = true, params Expression<Func<TEntity, object>>[] includes);    

    /// <summary>
    /// Update entity in the database.
    /// </summary>
    /// <param name="entity"></param>
    void Update(TEntity entity);
    
    /// <summary>
    /// Save changes to the database with an optional logical delete flag.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="needLogicalDelete"></param>
    /// <returns></returns>
    int SaveChanges(string userName, bool needLogicalDelete = false);
    
    /// <summary>
    /// Save changes to the database asynchronously with an optional logical delete flag.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="needLogicalDelete"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(string userName, bool needLogicalDelete = false);

    Task SessionSavechanges();
}