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
                        SELECT COUNT(users.user_name)
                        FROM Users users
                        WHERE users.user_name = @UserName
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
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            users.role_id as RoleId,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN Roles roles ON users.role_id = roles.Id
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
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            users.role_id as RoleId,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN Roles roles ON users.role_id = roles.Id
                        WHERE users.user_name = @Username
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
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            users.role_id as RoleId,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN Roles roles ON users.role_id = roles.Id
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
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            users.role_id as RoleId,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN Roles roles ON users.role_id = roles.Id
                    """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        var users = await QueryUsers(connection, command);
        
        return users;
    }

    public async Task<Guid> AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        INSERT INTO Users (id, user_name, first_name, last_name, password_hash, email, role_id)
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
                            user_name = @Username, 
                            first_name = @FirstName, 
                            last_name = @LastName, 
                            password_hash = @PasswordHash, 
                            email = @Email, 
                            role_id = @RoleId
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