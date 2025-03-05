using Application.Abstractions.Authentication;
using Dapper;
using Domain.RefreshTokens;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
{
    private readonly string? _connectionString;

    public RefreshTokenRepository(ICacheService cache, IOptions<PostgresOptions> postgresOptions) : base(cache)
    {
        _connectionString = postgresOptions.Value.GetConnectionString();
    }
    
    public async Task<RefreshToken?> GetTokenByUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var tokens = GetFromCache<RefreshToken>();
        
        if (tokens is not null) return tokens.FirstOrDefault(token => token.User.Id.Value == user.Id.Value);
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT refresh_tokens.*
                        FROM refresh_tokens
                        WHERE refresh_tokens.user_id = @UserId
                    """;
        
        var parameters = new { UserId = user.Id.Value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var token = await connection.QuerySingleOrDefaultAsync<RefreshTokenDto>(command);
            
            if (token == null) return null;

            return RefreshToken.Create(
                token.Id,
                token.Value,
                token.ExpiresUtc,
                user);
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    public async Task<RefreshToken?> GetTokenAsync(string value, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT 
                            refresh_tokens.id AS Id,
                            refresh_tokens.value AS Value,
                            refresh_tokens.expires_utc AS ExpiresUtc,
                            refresh_tokens.user_id AS UserId,
                            users.id AS Id,
                            users.user_name AS Username,
                            users.first_name AS FirstName,
                            users.last_name AS LastName,
                            users.email AS Email,
                            users.password_hash AS PasswordHash,
                            roles.id AS Id,
                            roles.name AS Name
                        FROM refresh_tokens
                            INNER JOIN users ON users.id = refresh_tokens.user_id
                            LEFT JOIN user_roles ON user_roles.user_id = users.id
                            LEFT JOIN roles ON roles.id = user_roles.role_id
                        WHERE refresh_tokens.value = @Value
                        ORDER BY refresh_tokens.expires_utc DESC
                    """;
        
        var parameters = new { Value = value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        var tokenDictionary = new Dictionary<Guid, RefreshToken>();
        
        try
        {
            var tokens = await connection
                .QueryAsync<RefreshTokenDto, UserDto, RoleDto, RefreshToken>(
                    command,
                    (token, user, role) =>
                    {
                        if (!tokenDictionary.TryGetValue(token.Id, out var refreshToken))
                        {
                            refreshToken = RefreshToken.Create(
                                token.Id,
                                token.Value,
                                token.ExpiresUtc,
                                User.CreateUser(
                                    user.Id, 
                                    user.Username, 
                                    user.FirstName, 
                                    user.LastName, 
                                    user.PasswordHash, 
                                    user.Email, 
                                    new List<RoleId>(),
                                    new List<UserPermissionId>()).Value);
                            
                            tokenDictionary.Add(token.Id, refreshToken);
                        }

                        if (role is not null)
                        {
                            refreshToken.User.RoleIds.Add(new RoleId(role.Id));
                        }
                        
                        return refreshToken;
                    },
                    splitOn: "Id,Id");
            
            var token = tokens.FirstOrDefault();
            
            if (token is not null && token.ExpiresUtc <= DateTime.UtcNow)
            {
                token = null;
            }
            
            return token;
        }
        catch (Exception e)
        {
            throw;
        }
    }    
    
    public async Task AddTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        INSERT INTO refresh_tokens (id, value, expires_utc, user_id)
                        VALUES (@Id, @Value, @ExpiresUtc, @UserId)
                    """;
        
        var parameters = new { Id = token.Id.Value, token.Value, token.ExpiresUtc, UserId = token.User.Id.Value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            await connection.ExecuteAsync(command);
        }
        catch (Exception e)
        {
            throw;
        }
    }    
    
    public async Task UpdateTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        UPDATE refresh_tokens
                        SET value = @Value, expires_utc = @ExpiresUtc
                        WHERE refresh_tokens.id = @Id
                    """;
        
        var parameters = new { token.Id, token.Value, token.ExpiresUtc };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        try
        {
            await connection.ExecuteAsync(command);
        }
        catch (Exception e)
        {
            throw;
        }
    }
}