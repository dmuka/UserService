using Core;
using Infrastructure.Caching.Interfaces;

namespace Infrastructure.Repositories;

public class BaseRepository(ICacheService cache)
{
    protected T? GetFromCache<T>(Func<T, bool> predicate) where T : Entity
    {
        var entities = GetFromCache<T>();
        
        return entities.FirstOrDefault(predicate);
    }

    protected IList<T> GetFromCache<T>() where T : Entity
    {
        return cache.Get<T>(nameof(T)) ?? new List<T>();
    }

    protected void RemoveFromCache<T>() where T : Entity
    {
        cache.Remove(nameof(T));
    }

    protected void CreateInCache<T>(ICollection<T> entities) where T : Entity
    {
        cache.Create(nameof(T), entities);
    }
}