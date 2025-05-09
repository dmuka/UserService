using Dapper;
using Domain.Permissions;
using Domain.UserPermissions;
using Domain.Users;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

/// <summary>
/// Provides an implementation for managing user permissions in a PostgreSQL database.
/// </summary>
public class UserPermissionsRepository(
    ILogger<UserPermissionsRepository> logger, 
    IOptions<PostgresOptions> postgresOptions)
    : IUserPermissionsRepository
{
    /// <summary>
    /// Represents the connection string used to establish a connection to the PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// This variable is initialized using the database configuration provided through <see cref="PostgresOptions"/>.
    /// It is used throughout the repository to create database connections for executing queries and commands.
    /// </remarks>
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    /// <summary>
    /// Retrieves a list of permissions associated with a specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user whose permissions are to be retrieved.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A list of <see cref="UserPermission"/> objects representing the permissions of the specified user.</returns>
    public async Task<IList<UserPermission>> GetPermissionsByUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                 SELECT 
                                     user_permissions.user_id AS "{nameof(UserPermissionDto.UserId)}",
                                     user_permissions.permission_id AS "{nameof(UserPermissionDto.PermissionId)}"
                                 FROM user_permissions
                                 WHERE user_permissions.user_id = @UserId
                             """;
        
        var parameters = new { UserId = userId.Value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var userPermissionDtos = await connection.QueryAsync<UserPermissionDto>(command);
            
            return userPermissionDtos.Select<UserPermissionDto, UserPermission>(permission => UserPermission.Create(
                    permission.Id, 
                    new UserId(permission.UserId), 
                    new PermissionId(permission.PermissionId)))
                .ToList();
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the user permissions by user id: {UserId}.", e, e.Message, userId.Value);
            throw;
        }
    }
}