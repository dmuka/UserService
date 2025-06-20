﻿using Dapper;
using Domain.Roles;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

/// <summary>
/// A repository responsible for managing user-role relationships in the system.
/// It provides methods to retrieve, update, and remove role assignments for users.
/// </summary>
public class UserRoleRepository(
    ICacheService cache,
    IRoleRepository roleRepository,
    ILogger<UserRoleRepository> logger,
    IOptions<PostgresOptions> postgresOptions)
    : BaseRepository(cache), IUserRoleRepository
{
    /// <summary>
    /// A constant string key used for caching or identifying a collection of user IDs.
    /// It serves as a prefix or base key in operations where user IDs need to be cached,
    /// retrieved, or manipulated in relation to roles.
    /// </summary>
    private const string UsersIdsKey = "users_ids";

    /// <summary>
    /// A constant key used for caching role ID information.
    /// This key is utilized to identify and retrieve cached data
    /// related to roles assigned to users, ensuring efficient access
    /// and reduced database calls within the repository.
    /// </summary>
    private const string RolesIdsKey = "roles_ids";

    /// <summary>
    /// A constant key used for caching role name information.
    /// This key is used to identify and retrieve cached data
    /// related to roles assigned to users, ensuring efficient access
    /// and reduced database calls within the repository.
    /// </summary>
    private const string RolesNamesKey = "roles_names";

    /// <summary>
    /// Represents the database connection string used to establish a connection with a PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// Fetched dynamically from the <see cref="PostgresOptions"/> configuration object.
    /// Used for executing database-related operations such as queries and transactions.
    /// </remarks>
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    public async Task<IList<Guid>> GetUsersIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var usersIds = GetFromCache<Guid>($"{UsersIdsKey}_{roleId}");
        
        if (usersIds is not null) return usersIds;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the users ids by role id: {RoleId}.", e, e.Message, roleId);
            throw;
        }
    }

    public async Task<IList<Guid>> GetRolesIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rolesIds = GetFromCache<Guid>($"{RolesIdsKey}_{userId}");
        
        if (rolesIds is not null) return rolesIds;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the roles ids by user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    public async Task<IList<string>> GetRolesNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rolesNames = GetFromCache<string>($"{RolesNamesKey}_{userId}");
        
        if (rolesNames is not null) return rolesNames;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = $"""
                                 SELECT 
                                     roles.name AS {nameof(Role.Name)}
                                 FROM user_roles
                                 INNER JOIN roles ON roles.id = user_roles.role_id
                                 WHERE user_roles.user_id = @UserId
                             """;
        
        var parameters = new { UserId = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = (await connection.QueryAsync<string>(command)).ToList();
            
            CreateInCache($"{RolesNamesKey}_{userId}", result);
            
            return result.ToList();
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the roles ids by user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    public async Task<int> UpdateUserRolesAsync(
        Guid userId, 
        IEnumerable<string> rolesNames,  
        CancellationToken cancellationToken)
    {
        var oldRolesNames = await GetRolesNamesByUserIdAsync(userId, cancellationToken);
        var newRolesNames = rolesNames.ToList();
        
        var toAdd = newRolesNames.Except(oldRolesNames).ToList();
        var toRemove = oldRolesNames.Except(newRolesNames).ToList();
        
        var updatedRoles = 0;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        if (toRemove.Count != 0) updatedRoles += await RemoveUserRolesAsync(userId, toRemove, connection, transaction);
        if (toAdd.Count != 0) updatedRoles += await AddUserRolesAsync(userId, toAdd, connection, transaction);
        
        await transaction.CommitAsync(cancellationToken);
        RemoveFromCache($"{UsersIdsKey}_{userId}");
        
        return updatedRoles; 
    }

    public async Task<int> RemoveAllUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        const string query = """
                                 DELETE FROM user_roles
                                 WHERE user_roles.user_id = @UserId
                             """;
        var parameters = new { UserId = userId };
        
        try
        {
            var result = await connection.ExecuteAsync(
                query, 
                parameters, 
                transaction: transaction);
            
            RemoveFromCache($"{RolesIdsKey}_{userId}");
            
            return result;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while removing all roles ids for user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    private async Task<int> AddUserRolesAsync(
        Guid userId, 
        IEnumerable<Guid> rolesIds,  
        NpgsqlConnection connection,
        NpgsqlTransaction transaction)
    {
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
            
            return result;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while adding the roles ids for user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    private async Task<int> AddUserRolesAsync(
        Guid userId, 
        IEnumerable<string> rolesNames,  
        NpgsqlConnection connection,
        NpgsqlTransaction transaction)
    {
        const string query = """
                                 INSERT INTO user_roles (user_id, role_id)
                                 VALUES (@UserId, @RoleId)
                             """;

        try
        {
            var rolesIds = new List<Guid>();

            foreach (var roleName in rolesNames)
            {
                var role = await roleRepository.GetRoleByNameAsync(roleName);
                
                if (role != null) rolesIds.Add(role.Id.Value);
                else logger.LogWarning("Role name '{RoleName}' not found, skipping.", roleName);
            }
            
            var result = await connection.ExecuteAsync(
                query, 
                rolesIds.Select(roleId => new { UserId = userId, RoleId = roleId }).ToArray(), 
                transaction: transaction);
            
            return result;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while adding the roles ids for user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    private async Task<int> RemoveUserRolesAsync(
        Guid userId, 
        IEnumerable<Guid> rolesIds,  
        NpgsqlConnection connection,
        NpgsqlTransaction transaction)
    {
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
            
            return result;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while removing the roles ids for user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    private async Task<int> RemoveUserRolesAsync(
        Guid userId, 
        IEnumerable<string> rolesNames,  
        NpgsqlConnection connection,
        NpgsqlTransaction transaction)
    {
        const string query = """
                                 DELETE FROM user_roles
                                 WHERE user_roles.user_id = @UserId 
                                   AND user_roles.role_id = @RoleId
                             """;

        try
        {            
            var rolesIds = new List<Guid>();

            foreach (var roleName in rolesNames)
            {
                var role = await roleRepository.GetRoleByNameAsync(roleName);
                
                if (role != null) rolesIds.Add(role.Id.Value);
                else logger.LogWarning("Role name '{RoleName}' not found, skipping.", roleName);
            }
            
            var result = await connection.ExecuteAsync(
                query,
                rolesIds.Select(roleId => new { UserId = userId, RoleId = roleId }), 
                transaction: transaction);
            
            return result;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while removing the roles ids for user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }
}