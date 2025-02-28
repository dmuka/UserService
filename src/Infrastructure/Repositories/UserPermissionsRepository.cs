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

public class UserPermissionsRepository(
    ILogger<UserPermissionsRepository> logger, 
    IOptions<PostgresOptions> postgresOptions)
    : IUserPermissionsRepository
{
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    public async Task<IList<UserPermission>> GetPermissionsByUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT user_permissions.*
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