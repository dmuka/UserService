using Core;
using Infrastructure.Caching.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Caching;

public class CacheService(IMemoryCache cache) : ICacheService
{
    public async Task<IList<T>> GetOrCreateAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<IList<T>>> getFromRepositoryAsync,
        CancellationToken cancellationToken,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null) where T : Entity
    {
        if (cache.TryGetValue(cacheKey, out IList<T>? cachedData) 
            && cachedData is not null)
        {
            return cachedData;
        }
        
        cachedData = await getFromRepositoryAsync(cancellationToken);
            
        var cacheEntryOptions = GetOptions(absoluteExpiration, slidingExpiration);
            
        cache.Set(cacheKey, cachedData, cacheEntryOptions);

        return cachedData ?? [];
    }
    
    public T? GetById<T>(string cacheKey, Guid id) where T : Entity
    {
        var entities = cache.Get<IList<T>>(cacheKey);
        
        var entity = entities?.FirstOrDefault(entity => entity.Id.Value == id);

        return entity;
    }
    
    public IList<T>? Get<T>(string cacheKey)
    {
        var entities = cache.Get<IList<T>>(cacheKey);

        return entities;
    }
    
    public T? GetEntity<T>(string cacheKey)
    {
        var entity = cache.Get<T>(cacheKey);

        return entity;
    }

    public void Create<T>(
        string cacheKey,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null)
    {
        if (cache.TryGetValue(cacheKey, out _)) return;
        
        var cacheEntryOptions = GetOptions(absoluteExpiration, slidingExpiration);
            
        cache.Set(cacheKey, value, cacheEntryOptions);
    }

    public void Remove(string cacheKey)
    {
        if (cache.TryGetValue(cacheKey, out _)) cache.Remove(cacheKey);
    }

    private static MemoryCacheEntryOptions GetOptions(
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(2),
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(5)
        };

        return cacheEntryOptions;
    }
}