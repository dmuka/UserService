using System.Data;
using Dapper;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRepository(IOptions<PostgresOptions> postgresOptions) : IUserRepository
{
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    public async Task<bool> IsUsernameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT COUNT(users.username)
                        FROM Users users
                        WHERE users.username = @UserName
                    """;
        
        var parameters = new { UserName = userName };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = await connection.ExecuteScalarAsync<int>(command);
            
            return result > 0;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT COUNT(users.email)
                        FROM Users users
                        WHERE users.email = @Email
                    """;
        
        var parameters = new { Email = email };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = await connection.ExecuteScalarAsync<int>(command);
            
            return result > 0;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT users.*, roles.*
                        FROM Users users
                            INNER JOIN Roles roles ON users.RoleId = roles.Id
                        WHERE users.Id = @UserId
                    """;
        
        var parameters = new { UserId = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = await QueryUsers(connection, command);
            
            var user = result.FirstOrDefault();
            
            return user;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT users.*, roles.*
                        FROM Users users
                            INNER JOIN Roles roles ON users.RoleId = roles.Id
                        WHERE users.Username = @Username
                    """;
        
        var parameters = new { Username = username };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        var result = await QueryUsers(connection, command);

        var user = result.FirstOrDefault();
        
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT users.*, roles.*
                        FROM Users users
                            INNER JOIN Roles roles ON users.RoleId = roles.Id
                        WHERE users.email = @Email
                    """;
        
        var parameters = new { Email = email };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        var result = await QueryUsers(connection, command);

        var user = result.FirstOrDefault();
        
        return user;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT users.*, roles.*
                        FROM Users users
                            INNER JOIN Roles roles ON users.RoleId = roles.Id
                    """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        var users = await QueryUsers(connection, command);
        
        return users;
    }

    public async Task<Guid> AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        INSERT INTO Users (Id, Username, FirstName, LastName, PasswordHash, Email, RoleId)
                        VALUES (@Id, @Username, @FirstName, @LastName, @PasswordHash, @Email, @RoleId)
                        RETURNING Id
                    """;
        
        var parameters = new
        {
            Id = user.Id.Value, 
            user.Username, 
            user.FirstName, 
            user.LastName, 
            PasswordHash = user.PasswordHash.Value, 
            Email = user.Email.Value, 
            RoleId = user.Role.Id.Value
        };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        var userId = await connection.ExecuteScalarAsync<Guid>(command);
        
        return userId;
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        var query = """
                        UPDATE Users 
                        SET 
                            Username = @Username, 
                            FirstName = @FirstName, 
                            LastName = @LastName, 
                            PasswordHash = @PasswordHash, 
                            Email = @Email, 
                            RoleId = @RoleId
                        WHERE Users.Id = @Id
                    """;
        
        var parameters = new { user.Id, user.Username, user.FirstName, user.LastName, user.PasswordHash, user.Email, user.Role.Id.Value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        await connection.ExecuteAsync(command);
    }

    private static async Task<IEnumerable<User>> QueryUsers(IDbConnection connection, CommandDefinition command)
    {
        var users = await connection.QueryAsync<UserDto, RoleDto, User>(
            command,
            (user, role) => 
                User.CreateUser(
                    user.Id, 
                    user.Username, 
                    user.FirstName, 
                    user.LastName, 
                    new PasswordHash(user.PasswordHash), 
                    new Email(user.Email), 
                    new Role(new RoleId(role.Id), role.Name)),
            splitOn: "Id");
        
        return users;
    }
}