using System.Data;
using Dapper;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Infrastructure.Caching.Interfaces;
using Infrastructure.Options.Db;
using Infrastructure.Repositories.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Repositories;

/// <summary>
/// A repository for managing user-related data within the PostgreSQL database.
/// Includes functionality for checking existence of usernames or emails, retrieving users by various identifiers, and managing CRUD operations.
/// Uses caching to enhance the performance of data retrieval.
/// </summary>
public class UserRepository(
    ICacheService cache, 
    IOptions<PostgresOptions> postgresOptions,
    ILogger<UserRepository> logger) : BaseRepository(cache), IUserRepository
{
    private readonly string? _connectionString = postgresOptions.Value.GetConnectionString();

    /// <summary>
    /// Checks if a username exists within the user repository.
    /// </summary>
    /// <param name="userName">The username to check for existence.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>Returns true if the username exists, otherwise false.</returns>
    public async Task<bool> IsUsernameExistsAsync(string userName, CancellationToken cancellationToken = default)
    {
        var users = GetFromCache<User, UserId>();
        
        if (users is not null) return users.Count(user => user.Username == userName) > 1;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the username: {UserName}.", e, e.Message, userName);
            throw;
        }
    }

    /// <summary>
    /// Checks if an email exists within the user repository.
    /// </summary>
    /// <param name="email">The email to check for existence.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>Returns true if the email exists, otherwise false.</returns>
    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var users = GetFromCache<User, UserId>();
        
        if (users is not null) return users.Count(user => user.Email == email) > 1;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = """
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
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the email: {Email}.", e, e.Message, email);
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = GetFirstFromCache<User, UserId>(user => user.Id.Value == userId);
        
        if (user is not null) return user;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                 SELECT
                                     users.id AS {nameof(User.Id)},
                                     users.user_name AS {nameof(User.Username)},
                                     users.first_name AS {nameof(User.FirstName)},
                                     users.last_name AS {nameof(User.LastName)},
                                     users.email AS {nameof(User.Email)},
                                     users.password_hash AS {nameof(User.PasswordHash)},
                                     roles.id AS {nameof(Role.Id)},
                                     roles.name AS {nameof(Role.Name)}
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
            
            user = result.FirstOrDefault();
            
            return user;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the user by id: {UserId}.", e, e.Message, userId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a user by their username from the user repository.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>Returns the user object if a user with the specified username exists; otherwise, null.</returns>
    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = GetFirstFromCache<User, UserId>(user => user.Username == username);
        
        if (user is not null) return user;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                  SELECT
                                      users.id AS {nameof(User.Id)},
                                      users.user_name AS {nameof(User.Username)},
                                      users.first_name AS {nameof(User.FirstName)},
                                      users.last_name AS {nameof(User.LastName)},
                                      users.email AS {nameof(User.Email)},
                                      users.password_hash AS {nameof(User.PasswordHash)},
                                      roles.id AS {nameof(Role.Id)},
                                      roles.name AS {nameof(Role.Name)}
                                  FROM Users users
                                      INNER JOIN user_roles UserRoles ON users.id = UserRoles.user_id 
                                      INNER JOIN roles ON UserRoles.role_id = roles.Id
                                  WHERE users.user_name = @Username
                              """;
        
        var parameters = new { Username = username };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = await QueryUsers(connection, command);

            user = result.FirstOrDefault();
        
            return user;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the user by username: {Username}.", e, e.Message, username);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a user by their email address from the repository.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>Returns the user object if found; otherwise, null.</returns>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = GetFirstFromCache<User, UserId>(user => user.Email.Value == email);
        
        if (user is not null) return user;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                 SELECT
                                     users.id AS {nameof(User.Id)},
                                     users.user_name AS {nameof(User.Username)},
                                     users.first_name AS {nameof(User.FirstName)},
                                     users.last_name AS {nameof(User.LastName)},
                                     users.email AS {nameof(User.Email)},
                                     users.password_hash AS {nameof(User.PasswordHash)},
                                     roles.id AS {nameof(Role.Id)},
                                     roles.name AS {nameof(Role.Name)}
                                 FROM Users users
                                     INNER JOIN user_roles UserRoles ON users.id = UserRoles.user_id 
                                     INNER JOIN roles ON UserRoles.role_id = roles.Id
                                 WHERE users.email = @Email
                             """;
        
        var parameters = new { Email = email };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var result = await QueryUsers(connection, command);

            user = result.FirstOrDefault();
        
            return user;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying the user by email: {Email}.", e, e.Message, email);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all users from the repository, including their roles and other relevant details.
    /// </summary>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>Returns a collection of users with their associated details.</returns>
    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = GetFromCache<User, UserId>();
        
        if (users is not null) return users;
        
        await using var connection = new NpgsqlConnection(_connectionString);
            
        const string query = $"""
                                 SELECT
                                     users.id AS {nameof(User.Id)},
                                     users.user_name AS {nameof(User.Username)},
                                     users.first_name AS {nameof(User.FirstName)},
                                     users.last_name AS {nameof(User.LastName)},
                                     users.email AS {nameof(User.Email)},
                                     users.password_hash AS {nameof(User.PasswordHash)},
                                     roles.id AS {nameof(Role.Id)},
                                     roles.name AS {nameof(Role.Name)}
                                 FROM Users users
                                     INNER JOIN user_roles UserRoles ON users.id = UserRoles.user_id 
                                     INNER JOIN roles ON UserRoles.role_id = roles.Id
                             """;
        
        var command = new CommandDefinition(query, cancellationToken: cancellationToken);

        try
        {
            users = (await QueryUsers(connection, command)).ToList();
            CreateInCache<User, UserId>(users);
        
            return users;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while querying all users.", e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user entity to add to the repository.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>Returns the unique identifier of the newly created user.</returns>
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

            foreach (var roleId in user.RoleIds)
            {
                command = new CommandDefinition(
                    query, 
                    new { UserId = user.Id.Value, RoleId = roleId.Value }, 
                    transaction, 
                    cancellationToken: cancellationToken);
                
                await connection.ExecuteAsync(command);
            }
            
            await transaction.CommitAsync(cancellationToken);
            RemoveFromCache<User, UserId>();
            
            return userId;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while adding a new user.", e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Updates the user details in the repository.
    /// </summary>
    /// <param name="user">The user entity containing updated data.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        
        const string query = """
                                 UPDATE Users 
                                 SET 
                                     user_name = @Username, 
                                     first_name = @FirstName, 
                                     last_name = @LastName, 
                                     password_hash = @PasswordHash, 
                                     email = @Email
                                 WHERE Users.Id = @Id
                             """;
        
        var parameters = new { Id = user.Id.Value, user.Username, user.FirstName, user.LastName, PasswordHash = user.PasswordHash.Value, Email = user.Email.Value };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            await connection.ExecuteAsync(command);
            RemoveFromCache<User, UserId>();
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while updating a user.", e, e.Message);
            throw;
        }
    }

    /// <summary>
    /// Removes a user from the repository by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to be removed.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>Returns the number of rows affected by the deletion operation.</returns>
    public async Task<int> RemoveUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        const string query = """
                                 DELETE
                                 FROM Users 
                                 WHERE Users.Id = @Id
                             """;
        
        var parameters = new { Id = userId };
        
        var command = new CommandDefinition(query, parameters: parameters, cancellationToken: cancellationToken);

        try
        {
            var rows = await connection.ExecuteAsync(command);
            RemoveFromCache<User, UserId>();

            return rows;
        }
        catch (Exception e)
        {
            logger.LogError("An error (exception: {exception}, message: {message}) occurred while removing a user by id.", e, e.Message);
            throw;
        }
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
                    userEntry = User.Create(
                        user.Id,
                        user.Username,
                        user.FirstName,
                        user.LastName,
                        user.PasswordHash,
                        user.Email,
                        new List<RoleId> { new (role.Id) },
                        new List<UserPermissionId>(),
                        new List<string>(),
                        user.IsMfaEnabled,
                        user.MfaSecret).Value;
                    userDictionary.Add(user.Id, userEntry);
                }
                else
                {
                    userEntry.AddRole(new RoleId(role.Id));
                }
                
                return userEntry;
            },
            splitOn: "Id");
        
        return users.Distinct();
    }
}