using Dapper;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRepository(IConfiguration configuration) : IUserRepository
{
    private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

    public async Task<User?> GetUserByIdAsync(ulong userId, CancellationToken cancellationToken = default)
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

        var result = await connection.QueryAsync<User, Role, User>(
            command,
            (user, role) =>
            {
                user.ChangeRole(role);
                return user;
            },
            splitOn: "Id");

        var user = result.FirstOrDefault();
        
        return user;
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

        var result = await connection.QueryAsync<User, Role, User>(
            command,
            (user, role) =>
            {
                user.ChangeRole(role);
                return user;
            },
            splitOn: "Id");

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

        var users = await connection.QueryAsync<User, Role, User>(
            query,
            (user, role) =>
            {
                user.ChangeRole(role);
                return user;
            },
            splitOn: "Id");
        
        return users;
    }

    public async Task<ulong> AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        INSERT INTO Users (Username, FirstName, LastName, PasswordHash, Email, RoleId)
                        VALUES (@Username, @FirstName, @LastName, @PasswordHash, @Email, @RoleId)
                        RETURNING Id
                    """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);
        
        var userId = await connection.ExecuteScalarAsync<ulong>(command);
        
        user.SetId(userId);
        
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
        
        var parameters = new { user.Id, user.Username, user.FirstName, user.LastName, user.PasswordHash, user.Email, user.RoleId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        await connection.ExecuteAsync(command);
    }
}