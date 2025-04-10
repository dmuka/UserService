using Dapper;
using Domain.Roles;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRoleRepository : BaseRepository, IUserRoleRepository
{
    private const string UsersIdsKey = "users_ids";
    private const string RolesIdsKey = "roles_ids";
    
    private readonly string? _connectionString;
    private readonly ILogger<UserRoleRepository> _logger;

    public UserRoleRepository(
        ICacheService cache, 
        ILogger<UserRoleRepository> logger, 
        IOptions<PostgresOptions> postgresOptions) : base(cache)
    {
        _connectionString = postgresOptions.Value.GetConnectionString();
        _logger = logger;
    }

    public async Task<IList<Guid>> GetUsersIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var usersIds = GetFromCache<Guid>($"{UsersIdsKey}_{roleId}");
        
        if (usersIds is not null) return usersIds;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT 
                            user_roles.user_id
                        FROM user_roles
                        WHERE user_roles.role_id = @RoleId
                    """;
        
        var parameters = new { RoleId = roleId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = (await connection.QueryAsync<Guid>(command)).ToList();
            
            CreateInCache($"{UsersIdsKey}_{roleId}", result);
            
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the users ids by role id: {RoleId}.", e, e.Message, roleId);
            throw;
        }
    }

    public async Task<IList<Guid>> GetRolesIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rolesIds = GetFromCache<Guid>($"{RolesIdsKey}_{userId}");
        
        if (rolesIds is not null) return rolesIds;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT 
                            user_roles.role_id
                        FROM user_roles
                        WHERE user_roles.user_id = @UserId
                    """;
        
        var parameters = new { UserId = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = (await connection.QueryAsync<Guid>(command)).ToList();
            
            CreateInCache($"{RolesIdsKey}_{userId}", result);
            
            return result.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the roles ids by user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    public async Task<int> UpdateUserRolesAsync(
        Guid userId, 
        IEnumerable<Guid> rolesIds,  
        CancellationToken cancellationToken)
    {
        var oldRolesIds = await GetRolesIdsByUserIdAsync(userId, cancellationToken);
        var newRolesIds = rolesIds.ToList();
        
        var toAdd = newRolesIds.Except(oldRolesIds).ToList();
        var toRemove = oldRolesIds.Except(newRolesIds).ToList();
        
        var updatedRoles = 0;
        if (toRemove.Count != 0) updatedRoles += await AddUserRolesAsync(userId, toAdd, cancellationToken);
        if (toAdd.Count != 0) updatedRoles += await RemoveUserRolesAsync(userId, toRemove, cancellationToken);
        
        RemoveFromCache($"{UsersIdsKey}_{userId}");
        
        return updatedRoles; 
    }

    public async Task<int> AddUserRolesAsync(
        Guid userId, 
        IEnumerable<Guid> rolesIds,  
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        const string query = """
                                 INSERT INTO user_roles (user_id, role_id)
                                 VALUES (@UserId, @RoleId)
                             """;

        try
        {
            var result = await connection.ExecuteAsync(
                query, 
                rolesIds.Select(roleId => new { UserId = userId, RoleId = roleId }).ToArray(), 
                transaction: transaction);
            
            RemoveFromCache($"{RolesIdsKey}_{userId}");
            
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("An error (exception: {exception}, message: {message}) occurred while adding the roles ids for user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    public async Task<int> RemoveUserRolesAsync(Guid userId, IEnumerable<Guid> rolesIds,  CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        const string query = """
                                 DELETE FROM user_roles
                                 WHERE user_roles.user_id = @UserId 
                                   AND user_roles.role_id = @RoleId
                             """;

        try
        {
            var result = await connection.ExecuteAsync(
                query, 
                rolesIds.Select(roleId => new { UserId = userId, RoleId = roleId }).ToArray(), 
                transaction: transaction);
            
            RemoveFromCache($"{RolesIdsKey}_{userId}");
            
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError("An error (exception: {exception}, message: {message}) occurred while removing the roles ids for user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }
}