using Dapper;
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
    /// Represents the database connection string used to establish a connection with a PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// Fetched dynamically from the <see cref="PostgresOptions"/> configuration object.
    /// Used for executing database-related operations such as queries and transactions.
    /// </remarks>
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    /// Asynchronously retrieves a list of user IDs associated with a specific role ID.
    /// <param name="roleId">
    /// The unique identifier of the role whose associated user IDs need to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token that can be used to propagate a cancellation request to the task.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of user IDs associated with the specified role ID.
    /// </returns>
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

    /// Asynchronously retrieves a list of role IDs associated with a specific user ID.
    /// <param name="userId">
    /// The unique identifier of the user whose associated role IDs need to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token that can be used to propagate a cancellation request to the task.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of role IDs associated with the specified user ID.
    /// </returns>
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

    /// Asynchronously updates the roles associated with a specific user by adding new roles and removing old ones as necessary.
    /// <param name="userId">
    /// The unique identifier of the user whose roles need to be updated.
    /// </param>
    /// <param name="rolesIds">
    /// A collection of role IDs that the user should be associated with after the update.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token that can be used to propagate a cancellation request to the task.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the total number of roles added or removed during the operation.
    /// </returns>
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
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        
        if (toRemove.Count != 0) updatedRoles += await RemoveUserRolesAsync(userId, toRemove, connection, transaction);
        if (toAdd.Count != 0) updatedRoles += await AddUserRolesAsync(userId, toAdd, connection, transaction);
        
        await transaction.CommitAsync(cancellationToken);
        RemoveFromCache($"{UsersIdsKey}_{userId}");
        
        return updatedRoles; 
    }

    /// Asynchronously removes all roles associated with a specific user ID.
    /// <param name="userId">
    /// The unique identifier of the user whose roles need to be removed.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token that can be used to propagate a cancellation request to the task.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of roles removed for the specified user ID.
    /// </returns>
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

    /// Asynchronously adds role associations for a user to the database and commits the changes within the provided database connection and transaction.
    /// <param name="userId">
    /// The unique identifier of the user for whom roles are being added.
    /// </param>
    /// <param name="rolesIds">
    /// A collection of unique identifiers for the roles to be associated with the user.
    /// </param>
    /// <param name="connection">
    /// The database connection to be used for executing the role-adding operation.
    /// </param>
    /// <param name="transaction">
    /// The transaction within which the operation is executed to ensure atomicity.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of roles successfully added for the user.
    /// </returns>
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

    /// Asynchronously removes specified roles from a user's role assignments in the database.
    /// <param name="userId">
    /// The unique identifier of the user whose roles need to be removed.
    /// </param>
    /// <param name="rolesIds">
    /// A collection of unique role IDs that should be removed from the user's assignments.
    /// </param>
    /// <param name="connection">
    /// The open database connection to be used for executing the query.
    /// </param>
    /// <param name="transaction">
    /// The database transaction within which the query will be executed.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the number of roles removed.
    /// </returns>
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
}