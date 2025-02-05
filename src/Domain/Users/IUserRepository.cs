namespace Domain.Users;

/// <summary>
/// Represents a repository for managing user data in a PostgreSQL database.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Asynchronously checks if a username exists in the database.
    /// </summary>
    /// <param name="userName">The username to check for existence.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing true if the username exists, otherwise false.</returns>
    /// <exception cref="Exception">Thrown if an error occurs during the database operation.</exception>
    Task<bool> IsUsernameExistsAsync(string userName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously checks if an email exists in the database.
    /// </summary>
    /// <param name="email">The email to check for existence.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing true if the email exists, otherwise false.</returns>
    /// <exception cref="Exception">Thrown if an error occurs during the database operation.</exception>
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously retrieves a user by the unique identifier from the database.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the user if found, otherwise null.</returns>
    /// <exception cref="Exception">Thrown if an error occurs during the database operation.</exception>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously retrieves a user by the username from the database.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the user if found, otherwise null.</returns>
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a user by the email address from the database.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the user if found, otherwise null.</returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all users from the database, including their associated roles.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a collection of users.</returns>
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds a new user to the database and returns the generated unique identifier.
    /// </summary>
    /// <param name="user">The user entity containing details to be added to the database.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the unique identifier of the newly added user.</returns>
    Task<Guid> AddUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates an existing user in the database with the provided user details.
    /// </summary>
    /// <param name="user">The user entity containing updated details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>    
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
}