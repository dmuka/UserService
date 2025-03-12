using Core;
using Infrastructure.Caching.Interfaces;

namespace Infrastructure.Repositories;

public class BaseRepository(ICacheService cache)
{
    protected T? GetFirstFromCache<T>(Func<T, bool> predicate) where T : Entity
    {
        var entities = GetFromCache<T>() ?? [];
        
        return entities.FirstOrDefault(predicate);
    }
    
    protected T? GetFirstFromCache<T>(string key, Func<T, bool> predicate)
    {
        var entities = GetFromCache<T>(key) ?? [];
        
        return entities.FirstOrDefault(predicate);
    }
    
    protected IList<T> GetFromCache<T>(Func<T, bool> predicate) where T : Entity
    {
        var entities = GetFromCache<T>() ?? [];
        
        return entities.Where(predicate).ToList();
    }
    
    protected IList<T> GetFromCache<T>(string key, Func<T, bool> predicate)
    {
        var entities = GetFromCache<T>(key) ?? [];
        
        return entities.Where(predicate).ToList();
    }

    protected IList<T>? GetFromCache<T>() where T : Entity
    {
        return cache.Get<T>(nameof(T));
    }

    protected IList<T>? GetFromCache<T>(string key) => cache.Get<T>(key);

    protected void RemoveFromCache<T>() where T : Entity
    {
        cache.Remove(nameof(T));
    }

    protected void RemoveFromCache(string key) => cache.Remove(key);

    protected void CreateInCache<T>(ICollection<T> entities) where T : Entity
    {
        cache.Create(nameof(T), entities);
    }

    protected void CreateInCache<T>(string key, ICollection<T> entities) => cache.Create(key, entities);
}