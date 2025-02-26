using System.Data;
using Dapper;
using Domain.Roles;
using Domain.Users;
using Domain.ValueObjects;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    private readonly string? _connectionString; 

    public UserRepository(ICacheService cache, IOptions<PostgresOptions> postgresOptions) : base(cache)
    {
        _connectionString = postgresOptions.Value.GetConnectionString();
    }

    public async Task<bool> IsUsernameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        var users = GetFromCache<User>();
        
        if (users is not null) return users.Count(user => user.Username == userName) > 1;
        
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
        var users = GetFromCache<User>();
        
        if (users is not null) return users.Count(user => user.Email == email) > 1;
        
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
        var users = GetFromCache<User>();
        
        if (users is not null) return users.FirstOrDefault(user => user.Id.Value == userId);
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN user_roles UserRoles ON users.id = UserRoles.user_id 
                            INNER JOIN roles ON UserRoles.role_id = roles.Id
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
        var users = GetFromCache<User>();
        
        if (users is not null) return users.FirstOrDefault(user => user.Username == username);
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN user_roles UserRoles ON users.id = UserRoles.user_id 
                            INNER JOIN roles ON UserRoles.role_id = roles.Id
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
        var users = GetFromCache<User>();
        
        if (users is not null) return users.FirstOrDefault(user => user.Email.Value == email);
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN user_roles UserRoles ON users.id = UserRoles.user_id 
                            INNER JOIN roles ON UserRoles.role_id = roles.Id
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
        var users = GetFromCache<User>();
        
        if (users is not null) return users;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        var query = """
                        SELECT
                            users.id,
                            users.user_name as Username,
                            users.first_name as FirstName,
                            users.last_name as LastName,
                            users.email as Email,
                            users.password_hash as PasswordHash,
                            roles.id,
                            roles.name
                        FROM Users users
                            INNER JOIN user_roles UserRoles ON users.id = UserRoles.user_id 
                            INNER JOIN roles ON UserRoles.role_id = roles.Id
                    """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        users = (await QueryUsers(connection, command)).ToList();
        CreateInCache(users);
        
        return users;
    }

    public async Task<Guid> AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        await connection.OpenAsync(cancellationToken);
        
        var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var query = """
                            INSERT INTO Users (id, user_name, first_name, last_name, password_hash, email)
                            VALUES (@Id, @Username, @FirstName, @LastName, @PasswordHash, @Email)
                            RETURNING Id
                        """;
            
            var parameters = new
            {
                Id = user.Id.Value, 
                user.Username, 
                user.FirstName, 
                user.LastName, 
                PasswordHash = user.PasswordHash.Value, 
                Email = user.Email.Value
            };
            
            var command = new CommandDefinition(query, parameters, transaction, cancellationToken: cancellationToken);
            
            var userId = await connection.ExecuteScalarAsync<Guid>(command);
            
            query = """
                        INSERT INTO user_roles (user_id, role_id)
                        VALUES (@UserId, @RoleId);
                    """;

            foreach (var role in user.Roles)
            {
                command = new CommandDefinition(
                    query, 
                    new { UserId = user.Id.Value, RoleId = role.Id.Value }, 
                    transaction, 
                    cancellationToken: cancellationToken);
                
                await connection.ExecuteAsync(command);
            }
            
            await transaction.CommitAsync(cancellationToken);
            RemoveFromCache<User>();
            
            return userId;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
                            email = @Email
                        WHERE Users.Id = @Id
                    """;
        
        var parameters = new { user.Id, user.Username, user.FirstName, user.LastName, user.PasswordHash, user.Email };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);
        
        await connection.ExecuteAsync(command);
        RemoveFromCache<User>();
    }

    private static async Task<IEnumerable<User>> QueryUsers(IDbConnection connection, CommandDefinition command)
    {
        var userDictionary = new Dictionary<Guid, User>();
        
        var users = await connection.QueryAsync<UserDto, RoleDto, User>(
            command,
            (user, role) =>
            {
                if (!userDictionary.TryGetValue(user.Id, out var userEntry))
                {
                    userEntry = User.CreateUser(
                        user.Id,
                        user.Username,
                        user.FirstName,
                        user.LastName,
                        new PasswordHash(user.PasswordHash),
                        new Email(user.Email),
                        new List<Role>());
                    userDictionary.Add(user.Id, userEntry);
                }
                userEntry.Roles.Add(Role.CreateRole(role.Id, role.Name));
                
                return userEntry;
            },
            splitOn: "Id");
        
        return users.Distinct();
    }
}