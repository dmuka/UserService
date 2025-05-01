using Application.Abstractions.Authentication;
using Dapper;
using Domain.RefreshTokens;
using Domain.Users;
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
        return await GetTokenByUserIdAsync(user.Id.Value, cancellationToken);
    }
    
    public async Task<RefreshToken?> GetTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = GetFromCache<RefreshToken>();
        
        if (tokens is not null) return tokens.FirstOrDefault(token => token.UserId.Value == userId);
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
                                 SELECT refresh_tokens.*
                                 FROM refresh_tokens
                                 WHERE refresh_tokens.user_id = @UserId
                             """;
        
        var parameters = new { UserId = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var token = await connection.QuerySingleOrDefaultAsync<RefreshTokenDto>(command);
            
            if (token == null) return null;

            return RefreshToken.Create(
                token.Id,
                token.Value,
                token.ExpiresUtc,
                new UserId(userId)).Value;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    public async Task<RefreshToken?> GetTokenByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tokens = GetFromCache<RefreshToken>();
        
        if (tokens is not null) return tokens.FirstOrDefault(token => token.Id.Value == id);
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
                                 SELECT
                                     refresh_tokens.id AS Id,
                                     refresh_tokens.value AS Value,
                                     refresh_tokens.expires_utc AS ExpiresUtc,
                                     refresh_tokens.user_id AS UserId
                                 FROM refresh_tokens
                                 WHERE refresh_tokens.id = @Id
                             """;
        
        var parameters = new { Id = id };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var token = (await connection.QueryAsync<RefreshTokenDto>(command))
                .OrderByDescending(token => token.ExpiresUtc)
                .FirstOrDefault();
            
            if (token == null) return null;

            return RefreshToken.Create(
                token.Id,
                token.Value,
                token.ExpiresUtc,
                new UserId(token.UserId)).Value;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    public async Task<RefreshToken?> GetTokenAsync(string value, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
                                 SELECT 
                                     refresh_tokens.id AS Id,
                                     refresh_tokens.value AS Value,
                                     refresh_tokens.expires_utc AS ExpiresUtc,
                                     refresh_tokens.user_id AS UserId
                                 FROM refresh_tokens
                                 WHERE refresh_tokens.value = @Value
                                 ORDER BY refresh_tokens.expires_utc DESC
                             """;
        
        var parameters = new { Value = value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        try
        {
            var dtos = (await connection.QueryAsync<RefreshTokenDto>(command)).ToList();

            var token = dtos.Select(dto =>
            {
                var result = RefreshToken.Create(dto.Id, dto.Value, dto.ExpiresUtc, new UserId(dto.UserId));
                
                return result.IsSuccess ? result.Value : null;
            }).FirstOrDefault();
            
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
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
                                 INSERT INTO refresh_tokens (id, value, expires_utc, user_id)
                                 VALUES (@Id, @Value, @ExpiresUtc, @UserId)
                             """;
        
        var parameters = new { Id = token.Id.Value, token.Value, token.ExpiresUtc, UserId = token.UserId.Value };
        
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
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
                                 UPDATE refresh_tokens
                                 SET value = @Value, expires_utc = @ExpiresUtc
                                 WHERE refresh_tokens.id = @Id
                             """;
        
        var parameters = new { Id = token.Id.Value, token.Value, token.ExpiresUtc };
        
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
    
    public async Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = """
                                 DELETE
                                 FROM refresh_tokens
                                 WHERE refresh_tokens.expires_utc < @ExpiresUtc
                             """;
        
        var parameters = new { ExpiresUtc = DateTime.UtcNow };
        
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