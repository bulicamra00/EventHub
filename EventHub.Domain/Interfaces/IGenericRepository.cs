using System.Linq.Expressions;

namespace EventHub.Domain.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllAsync(params string[] includes);
    
    IQueryable<T> GetQueryable(params string[] includes);
    
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null, 
        params string[] includes);
    
    Task<T?> GetByConditionAsync(Expression<Func<T, bool>> predicate);

    Task<IEnumerable<T>> GetListByConditionAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetListByConditionAsync(Expression<Func<T, bool>> predicate, params string[] includes);

    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);

    void RemoveRange(IEnumerable<T> entities);
}