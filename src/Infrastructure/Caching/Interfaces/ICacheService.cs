using Core;

namespace Infrastructure.Caching.Interfaces;

public interface ICacheService
{
    Task<IList<T>> GetOrCreateAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<IList<T>>> getFromRepositoryAsync,
        CancellationToken cancellationToken,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null) where T : Entity<TypedId>;

    IList<T>? Get<T>(string cacheKey);
    T? GetEntity<T>(string cacheKey);

    public void Create<T>(
        string cacheKey,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null);

    T? GetById<T>(string cacheKey, Guid id) where T : Entity<TypedId>;

    void Remove(string cacheKey);
}