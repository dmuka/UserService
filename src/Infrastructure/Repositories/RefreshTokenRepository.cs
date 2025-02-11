using Application.Abstractions.Authentication;
using Dapper;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository(IOptions<PostgresOptions> postgresOptions) : IRefreshTokenRepository
{
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    public async Task<RefreshToken?> GetTokenByUserAsync(User user, CancellationToken cancellationToken = default)
    {
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
            
            return new RefreshToken
            {
                Id = token.Id,
                Value = token.Value,
                ExpiresUtc = token.ExpiresUtc,
                User = user
            };
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
                        SELECT refresh_tokens.*, users.*
                        FROM refresh_tokens
                        INNER JOIN users ON users.id = refresh_tokens.user_id
                        WHERE refresh_tokens.value = @Value
                    """;
        
        var parameters = new { Value = value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var tokens = await connection
                .QueryAsync<RefreshTokenDto, UserDto, RoleDto, RefreshToken>(
                    command,
                    (token, user, role) => new RefreshToken
                    {
                        Id = token.Id,
                        Value = token.Value,
                        ExpiresUtc = token.ExpiresUtc,
                        User = User.CreateUser(
                            user.Id, 
                            user.Username, 
                            user.FirstName, 
                            user.LastName, 
                            new PasswordHash(user.PasswordHash), 
                            new Email(user.Email), 
                            new List<Role>())
                    });
            
            var token = tokens.FirstOrDefault();
            
            if (token == null || token.ExpiresUtc <= DateTime.UtcNow)
            {
                throw new ApplicationException("Refresh token has expired.");
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
        
        var parameters = new { token.Id, token.Value, token.ExpiresUtc, UserId = token.User.Id.Value };
        
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