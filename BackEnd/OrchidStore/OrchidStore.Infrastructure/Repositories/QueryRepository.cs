using System.Linq.Expressions;
using Marten;
using OrchidStore.Application.Repositories;

namespace OrchidStore.Infrastructure.Repositories;

public class QueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
{
    private readonly IDocumentSession _documentSession;

    public QueryRepository(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    /// <summary>
    /// Find entities as asynchronous
    /// </summary>
    /// <returns></returns>
    public async Task<List<TEntity>> FindAllAsync()
    {
        var result = await _documentSession.Query<TEntity>().ToListAsync();
        return result.ToList();
    }   
    
    /// <summary>
    /// Find entities as asynchronous
    /// </summary>
    /// <returns></returns>
    public async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        var result = await _documentSession.Query<TEntity>().Where(predicate).ToListAsync();
        return result.ToList();
    }

    /// <summary>
    /// Find all entities
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TEntity> FindAll()
    {
        return _documentSession.Query<TEntity>().ToList();
    }

    /// <summary>
    /// Find entity by predicate
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _documentSession.Query<TEntity>().FirstOrDefaultAsync(predicate);
    }
}