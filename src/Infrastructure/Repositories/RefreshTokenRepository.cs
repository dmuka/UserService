using Application.Abstractions.Authentication;
using Dapper;
using Domain.RefreshTokens;
using Domain.Users;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

/// <summary>
/// Provides functionality for managing refresh tokens within the system.
/// This repository supports operations such as retrieving, adding, updating, and removing refresh tokens.
/// It utilizes caching for optimized performance and interacts with the database when necessary.
/// </summary>
public class RefreshTokenRepository(
    ICacheService cache,
    IOptions<PostgresOptions> postgresOptions,
    ILogger<RefreshTokenRepository> logger)
    : BaseRepository(cache), IRefreshTokenRepository
{
    /// <summary>
    /// Represents the connection string used to establish a database connection.
    /// </summary>
    /// <remarks>
    /// This variable holds the connection string retrieved from the configured
    /// database options (PostgresOptions). It is used to create a connection
    /// to the PostgreSQL database for executing queries and retrieving data.
    /// </remarks>
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    /// <summary>
    /// Asynchronously retrieves a refresh token associated with the specified user from the repository.
    /// </summary>
    /// <param name="user">The user whose associated refresh token needs to be retrieved.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete, or the default value if not provided.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the refresh token associated with the user, or null if no token is found.</returns>
    public async Task<RefreshToken?> GetTokenByUserAsync(User user, CancellationToken cancellationToken = default)
    {
        return await GetTokenByUserIdAsync(user.Id.Value, cancellationToken);
    }

    /// <summary>
    /// Retrieves a refresh token associated with a specific user ID asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the refresh token is to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the refresh token associated with the given user ID,
    /// or null if no token is found.
    /// </returns>
    public async Task<RefreshToken?> GetTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = GetFromCache<RefreshToken, RefreshTokenId>();
        
        if (tokens is not null) return tokens.FirstOrDefault(token => token.UserId.Value == userId);
        
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
            
        const string query = $"""
                                 SELECT 
                                     refresh_tokens.id AS {nameof(RefreshToken.Id)},
                                     refresh_tokens.expires_utc AS {nameof(RefreshToken.ExpiresUtc)},
                                     refresh_tokens.value AS {nameof(RefreshToken.Value)},
                                     refresh_tokens.user_id AS {nameof(RefreshToken.UserId)}
                                 FROM refresh_tokens
                                 WHERE refresh_tokens.user_id = @UserId
                             """;
        
        var parameters = new { UserId = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var tokenDtos = await connection.QueryAsync<RefreshTokenDto>(command);
            
            var token = tokenDtos.OrderByDescending(t => t.ExpiresUtc).FirstOrDefault();
            
            if (token == null) return null;

            return RefreshToken.Create(
                token.Id,
                token.Value,
                token.ExpiresUtc,
                new UserId(userId)).Value;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the refresh token by user id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a refresh token by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the refresh token.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// The refresh token associated with the specified identifier, or null if no matching token is found.
    /// </returns>
    public async Task<RefreshToken?> GetTokenByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tokens = GetFromCache<RefreshToken, RefreshTokenId>();
        
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the refresh token by id: {Id}.", e, e.Message, id);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a refresh token based on its value from the database.
    /// </summary>
    /// <param name="value">The value of the refresh token to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="RefreshToken"/> object if found, otherwise null.</returns>
    /// <exception cref="Exception">Thrown when an error occurs while querying the database.</exception>
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the refresh token by value: {Value}.", e, e.Message, value);
            throw;
        }
    }

    /// <summary>
    /// Adds a new refresh token to the data store.
    /// </summary>
    /// <param name="token">The refresh token to be added.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A Task that represents the asynchronous operation.</returns>
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while adding the refresh token.", e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing refresh token with new values in the data store.
    /// </summary>
    /// <param name="token">The refresh token object containing updated values.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while updating the refresh token.", e, e.Message);
            throw;
        }
    }

    /// Removes all expired refresh tokens from the database.
    /// <param name="cancellationToken">
    /// An optional cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A Task that represents the asynchronous operation of removing expired refresh tokens.
    /// </returns>
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while removing expired refresh tokens.", e, e.Message);
            throw;
        }
    }
}