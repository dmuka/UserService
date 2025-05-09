using Core;
using Infrastructure.Caching.Interfaces;

namespace Infrastructure.Repositories;

/// BaseRepository class provides caching functionality for derived repository classes.
/// It encapsulates methods for retrieving, creating, and removing entities from an in-memory cache.
/// This class is designed to be used as a base class for repositories that require caching functionality.
public class BaseRepository(ICacheService cache)
{
    /// Retrieves the first entity from the cache that matches the specified condition.
    /// <typeparam name="T">The type of the entity to retrieve from the cache. The type must inherit from Entity<TId>.</typeparam>
    /// <typeparam name="TId">The type of the entity's identifier. The type must inherit from TypedId.</typeparam>
    /// <param name="predicate">A function to test each entity for a condition.</param>
    /// <returns>The first entity from the cache that matches the specified condition, or null if no such entity exists.
    /// If the cache does not contain the relevant collection, the method returns null as well.</returns>
    protected T? GetFirstFromCache<T, TId>(Func<T, bool> predicate) 
        where T : Entity<TId>
        where TId : TypedId
    {
        var entities = GetFromCache<T, TId>() ?? [];
        
        return entities.FirstOrDefault(predicate);
    }

    /// Retrieves the first entity from the cache that satisfies the specified condition.
    /// <typeparam name="T">The type of the entity to retrieve from the cache.</typeparam>
    /// <param name="key">The cache key that identifies the collection of entities.</param>
    /// <param name="predicate">A function to test each entity for a specific condition.</param>
    /// <returns>The first entity that matches the given condition, or null if none is found or if the cache does not contain the relevant collection.</returns>
    protected T? GetFirstFromCache<T>(string key, Func<T, bool> predicate)
    {
        var entities = GetFromCache<T>(key) ?? [];
        
        return entities.FirstOrDefault(predicate);
    }

    /// Retrieves a list of entities from the cache that match the specified predicate.
    /// <typeparam name="T">The type of the entity to retrieve from the cache. The type must inherit from Entity<TId>.</typeparam>
    /// <typeparam name="TId">The type of the entity's identifier. The type must inherit from TypedId.</typeparam>
    /// <param name="predicate">A function to filter the entities in the cache based on a given condition.</param>
    /// <returns>A list of entities from the cache that satisfy the specified predicate. If the cache
    /// is empty or does not contain the relevant collection, the method returns an empty list.</returns>
    protected IList<T> GetFromCache<T, TId>(Func<T, bool> predicate) 
        where T : Entity<TId>
        where TId : TypedId
    {
        var entities = GetFromCache<T, TId>() ?? [];
        
        return entities.Where(predicate).ToList();
    }

    /// Retrieves entities from the cache based on the specified key and filters them by the provided predicate.
    /// <typeparam name="T">The type of the entity to retrieve from the cache.</typeparam>
    /// <param name="key">The key used to locate the cache entry.</param>
    /// <param name="predicate">A function to test each entity for a condition.</param>
    /// <returns>A list of entities from the cache that satisfy the specified condition, or an empty list if no match is found or the cache does not contain the relevant entry.</returns>
    protected IList<T> GetFromCache<T>(string key, Func<T, bool> predicate)
    {
        var entities = GetFromCache<T>(key) ?? [];
        
        return entities.Where(predicate).ToList();
    }

    /// Retrieves a collection of entities from the cache for the specified type.
    /// <typeparam name="T">The type of the entities to retrieve from the cache. The type must inherit from Entity<TId>.</typeparam>
    /// <typeparam name="TId">The type of the entities' identifier. The type must inherit from TypedId.</typeparam>
    /// <returns>A list of entities with the specified type retrieved from the cache, or null if the cache does not contain a collection for the specified type.</returns>
    protected IList<T>? GetFromCache<T, TId>() 
        where T : Entity<TId>
        where TId : TypedId
    {
        return cache.Get<T>(typeof(T).Name);
    }

    /// Retrieves a collection of entities of the specified type from the cache based on the given key.
    /// <typeparam name="T">The type of entities to retrieve from the cache.</typeparam>
    /// <param name="key">The cache key associated with the collection of entities.</param>
    /// <returns>A collection of entities of type <typeparamref name="T"/> retrieved from the cache.
    /// Returns null if no entities are found in the cache for the provided key.</returns>
    protected IList<T>? GetFromCache<T>(string key) => cache.Get<T>(key);

    /// Removes the cached data for the specified type.
    /// The cache key used for removal is derived from the name of the type.
    /// This operation clears all cached items associated with the given type.
    /// <typeparam name="T">The type of the entity whose cache should be removed. The type must inherit from Entity<TId>.</typeparam>
    /// <typeparam name="TId">The type of the identifier for the entity. The type must inherit from TypedId.</typeparam>
    protected void RemoveFromCache<T, TId>() 
        where T : Entity<TId>
        where TId : TypedId
    {
        cache.Remove(nameof(T));
    }

    /// Removes an entry from the cache identified by the specified key.
    /// <param name="key">The key corresponding to the cache entry to be removed.</param>
    protected void RemoveFromCache(string key) => cache.Remove(key);

    /// Creates a collection of entities in the cache under a key derived from the entity type name.
    /// <typeparam name="T">The type of the entities to be stored in the cache. The type must inherit from Entity<TId>.</typeparam>
    /// <typeparam name="TId">The type of the entity's identifier. The type must inherit from TypedId.</typeparam>
    /// <param name="entities">The collection of entities to be stored in the cache.</param>
    protected void CreateInCache<T, TId>(ICollection<T> entities) 
        where T : Entity<TId>
        where TId : TypedId
    {
        cache.Create(nameof(T), entities);
    }

    /// Creates a collection of entities in the cache with the specified key.
    /// This method stores a given collection of entities in the cache for later retrieval.
    /// <typeparam name="T">The type of the entities to create in the cache.</typeparam>
    /// <param name="key">The unique key identifying the cached collection.</param>
    /// <param name="entities">The collection of entities to save in the cache.</param>
    protected void CreateInCache<T>(string key, ICollection<T> entities) => cache.Create(key, entities);
}