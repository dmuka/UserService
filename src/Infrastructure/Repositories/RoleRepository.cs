using Dapper;
using Domain.Roles;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

/// <summary>
/// Represents a repository for managing role-related operations, such as querying,
/// updating, adding, or removing roles in a storage system. It relies on caching
/// for optimization and supports PostgreSQL database interactions.
/// </summary>
public class RoleRepository(
    ICacheService cache,
    ILogger<RoleRepository> logger,
    IOptions<PostgresOptions> postgresOptions)
    : BaseRepository(cache), IRoleRepository
{
    /// <summary>
    /// Represents the connection string used to establish a connection to the PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// This value is retrieved from the configuration options provided by <see cref="PostgresOptions"/>.
    /// </remarks>
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    /// Checks if a role name exists in the data source or cache.
    /// <param name="roleName">The name of the role to check.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the role name exists; otherwise, false.</returns>
    public async Task<bool> IsRoleNameExistsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var roles = GetFromCache<Role, RoleId>();
        
        if (roles is not null) return roles.Count(role => role.Name == roleName) > 1;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the role name: {RoleName}.", e, e.Message, roleName);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a role by its unique identifier asynchronously.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to retrieve.</param>
    /// <param name="cancellationToken">An optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="Role"/> object if found; otherwise, null.</returns>
    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = GetFirstFromCache<Role, RoleId>(role => role.Id.Value == roleId);
        if (role is not null) return role;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                 SELECT 
                                     roles.id AS {nameof(Role.Id)},
                                     roles.name AS {nameof(Role.Name)}
                                 FROM roles
                                 WHERE roles.Id = @RoleId
                             """;
        
        var parameters = new { RoleId = roleId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var roleDto = await connection.QuerySingleOrDefaultAsync<RoleDto>(command);
            if (roleDto is null) return null;

            RemoveFromCache<Role, RoleId>();
            
            return Role.Create(roleDto.Id, roleDto.Name).Value;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the role by id: {RoleId}.", e, e.Message, roleId);
            throw;
        }
    }

    /// <summary>
    /// Removes a role from the database by its unique identifier.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to be removed.</param>
    /// <param name="cancellationToken">An optional cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>The number of rows affected in the database as a result of the delete operation.</returns>
    public async Task<int> RemoveRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
                                 DELETE FROM roles
                                 WHERE roles.Id = @RoleId
                             """;
        
        var parameters = new { RoleId = roleId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var rowsCount = await connection.ExecuteAsync(command);

            RemoveFromCache<Role, RoleId>();
            
            return rowsCount;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while deleting the role by id: {RoleId}.", e, e.Message, roleId);
            throw;
        }
    }

    /// Asynchronously retrieves a list of roles associated with a specified user ID.
    /// <param name="userId">The unique identifier of the user whose roles are to be retrieved.</param>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of roles associated with the specified user ID.</returns>
    public async Task<IList<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                  SELECT
                                      roles.id AS {nameof(Role.Id)},
                                      roles.name AS {nameof(Role.Name)}
                                  FROM roles
                                  LEFT JOIN user_roles ON roles.Id = user_roles.role_id
                                  WHERE user_roles.user_id = @UserId
                              """;
        
        var parameters = new { UserId = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var roleDtos = await connection.QueryAsync<RoleDto>(command);
            
            return roleDtos.Select(dto => Role.Create(dto.Id, dto.Name).Value).ToList();
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the roles by user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    /// Retrieves a role by its name asynchronously from the repository.
    /// The method first checks the cache for the role, and if not found, queries the database.
    /// If found, the role is returned; otherwise, null is returned.
    /// Logs any errors that occur during the database query and rethrows the exception.
    /// <param name="roleName">The name of the role to retrieve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The role found with the specified name, or null if no such role exists.</returns>
    public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = GetFirstFromCache<Role, RoleId>(role => role.Name == roleName);
        if (role is not null) return role;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                  SELECT
                                      roles.id AS {nameof(Role.Id)},
                                      roles.name AS {nameof(Role.Name)}
                                  FROM roles
                                  WHERE roles.name = @RoleName
                              """;
        
        var parameters = new { RoleName = roleName };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        try
        {
            var roleDto = await connection.QuerySingleOrDefaultAsync<RoleDto>(command);
            RemoveFromCache<Role, RoleId>();

            return roleDto is not null ? Role.Create(roleDto.Id, roleDto.Name).Value : null;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the role by name: {RoleName}.", e, e.Message, roleName);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all roles from the database or from the cache if available.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of roles.</returns>
    public async Task<IList<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = GetFromCache<Role, RoleId>();
        
        if (roles is not null) return roles;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                 SELECT 
                                     roles.id AS "{nameof(RoleDto.Id)}",
                                     roles.Name AS "{nameof(RoleDto.Name)}"
                                 FROM roles
                             """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        roles = (await connection.QueryAsync<RoleDto>(command))
            .Select(dto => Role.Create(dto.Id, dto.Name).Value).ToList();
        CreateInCache<Role, RoleId>(roles);
        
        return roles;
    }

    /// <summary>
    /// Adds a new role to the database and returns the unique identifier of the created role.
    /// </summary>
    /// <param name="role">The role entity containing the details of the role to be added.</param>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>Returns the unique identifier of the newly created role.</returns>
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
        RemoveFromCache<Role, RoleId>();
        
        return roleId;
    }

    /// <summary>
    /// Updates the specified role in the database.
    /// </summary>
    /// <param name="role">The role to be updated, containing the updated data.</param>
    /// <param name="cancellationToken">The cancellation token to observe for operation cancellation.</param>
    /// <returns>The number of rows affected by the update operation.</returns>
    public async Task<int> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        const string query = """
                                 UPDATE roles 
                                 SET 
                                     name = @Name
                                 WHERE roles.id = @Id
                             """;
        
        var parameters = new { Id = role.Id.Value, role.Name };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        var rows = await connection.ExecuteAsync(command);
        RemoveFromCache<Role, RoleId>();

        return rows;
    }
}