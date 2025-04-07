using Dapper;
using Domain.Roles;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class RoleRepository : BaseRepository, IRoleRepository
{
    private readonly string? _connectionString;
    private readonly ILogger<RoleRepository> _logger;

    public RoleRepository(
        ICacheService cache, 
        ILogger<RoleRepository> logger, 
        IOptions<PostgresOptions> postgresOptions) : base(cache)
    {
        _connectionString = postgresOptions.Value.GetConnectionString();
        _logger = logger;
    }

    public async Task<bool> IsRoleNameExistsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var roles = GetFromCache<Role>();
        
        if (roles is not null) return roles.Count(role => role.Name == roleName) > 1;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT COUNT(roles.name)
                        FROM roles
                        WHERE roles.name = @RoleName
                    """;
        
        var parameters = new { RoleName = roleName };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = await connection.ExecuteScalarAsync<int>(command);
            
            return result > 0;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = GetFirstFromCache<Role>(role => role.Id.Value == roleId);
        if (role is not null) return role;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT roles.*
                        FROM roles
                        WHERE roles.Id = @RoleId
                    """;
        
        var parameters = new { RoleId = roleId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var roleDto = await connection.QuerySingleOrDefaultAsync<RoleDto>(command);
            if (roleDto is null) return null;

            RemoveFromCache<Role>();
            
            return Role.Create(roleDto.Id, roleDto.Name);
        }
        catch (Exception e)
        {
            _logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the role by id: {RoleId}.", e, e.Message, roleId);
            throw;
        }
    }
    
    public async Task<int> RemoveRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        DELETE FROM roles
                        WHERE roles.Id = @RoleId
                    """;
        
        var parameters = new { RoleId = roleId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var rowsCount = await connection.ExecuteAsync(command);

            RemoveFromCache<Role>();
            
            return rowsCount;
        }
        catch (Exception e)
        {
            _logger.LogError("An error (exception: {exception}, message: {message}) occurred while deleting the role by id: {RoleId}.", e, e.Message, roleId);
            throw;
        }
    }
    
    public async Task<IList<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT roles.*
                        FROM roles
                        LEFT JOIN user_roles ON roles.Id = user_roles.role_id
                        WHERE user_roles.user_id = @UserId
                    """;
        
        var parameters = new { UserId = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var roleDtos = await connection.QueryAsync<RoleDto>(command);
            
            return roleDtos.Select(dto => Role.Create(dto.Id, dto.Name)).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the roles by user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = GetFirstFromCache<Role>(role => role.Name == roleName);
        if (role is not null) return role;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT roles.*
                        FROM roles
                        WHERE roles.name = @RoleName
                    """;
        
        var parameters = new { RoleName = roleName };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        try
        {
            var roleDto = await connection.QuerySingleOrDefaultAsync<RoleDto>(command);
            RemoveFromCache<Role>();

            return roleDto is not null ? Role.Create(roleDto.Id, roleDto.Name) : null;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<IList<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = GetFromCache<Role>();
        
        if (roles is not null) return roles;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
                                 SELECT roles.*
                                 FROM roles
                             """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        roles = (await connection.QueryAsync<RoleDto>(command))
            .Select(dto => Role.Create(dto.Id, dto.Name)).ToList();
        CreateInCache(roles);
        
        return roles;
    }

    public async Task<Guid> AddRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
                                 INSERT INTO roles (name)
                                 VALUES (@Name)
                                 RETURNING Id
                             """;
        var parameters = new { role.Name };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        var roleId = await connection.ExecuteScalarAsync<Guid>(command);
        RemoveFromCache<Role>();
        
        return roleId;
    }

    public async Task UpdateRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        const string query = """
                                 UPDATE roles 
                                 SET 
                                     name = @Name
                                 WHERE roles.id = @Id
                             """;
        
        var parameters = new { role.Name };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        await connection.ExecuteAsync(command);
        RemoveFromCache<Role>();
    }
}