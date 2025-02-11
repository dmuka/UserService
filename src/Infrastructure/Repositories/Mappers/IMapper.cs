using System.Reflection;
using Core;
using Infrastructure.Repositories.Dtos;

namespace Infrastructure.Repositories.Mappers;

public interface IMapper<TEntity, TDto> 
    where TEntity : Entity
    where TDto : IDto
{
    public TDto ToDto(TEntity entity);
    public TEntity ToEntity(TDto dto);
}