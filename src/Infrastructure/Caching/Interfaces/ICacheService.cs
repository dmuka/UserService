using Core;

namespace Infrastructure.Caching.Interfaces;

public interface ICacheService
{
    Task<IList<T>> GetOrCreateAsync<T>(
        string cacheKey,
        Func<Task<IList<T>>> getFromRepositoryAsync,
        CancellationToken cancellationToken,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null) where T : Entity;

    IList<T> Get<T>(string cacheKey) where T : Entity;
    T? GetEntity<T>(string cacheKey) where T : Entity;

    public void Create<T>(
        string cacheKey,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null);

    T? GetById<T>(string cacheKey, Guid id) where T : Entity;

    void Remove(string cacheKey);
}