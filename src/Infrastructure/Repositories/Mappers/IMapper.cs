using Core;
using Infrastructure.Repositories.Dtos;

namespace Infrastructure.Repositories.Mappers;

/// <summary>
/// Interface for mapping between domain entities with custom ID types and data transfer objects.
/// </summary>
/// <typeparam name="TEntity">The domain entity type.</typeparam>
/// <typeparam name="TId">The ID type for the entity.</typeparam>
/// <typeparam name="TDto">The data transfer object type.</typeparam>
public interface IMapper<TEntity, TId, TDto>
    where TEntity : Entity<TId>
    where TId : TypedId
    where TDto : IDto
{
    /// <summary>
    /// Maps a domain entity to a data transfer object.
    /// </summary>
    /// <param name="entity">The domain entity to map.</param>
    /// <returns>The mapped data transfer object.</returns>
    TDto ToDto(TEntity entity);

    /// <summary>
    /// Maps a data transfer object to a domain entity.
    /// </summary>
    /// <param name="dto">The data transfer object to map.</param>
    /// <returns>The mapped domain entity.</returns>
    TEntity ToEntity(TDto dto);
}