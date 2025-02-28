using Core;

namespace Domain.Permissions;

public class Permission : Entity
{
    public PermissionId Id { get; private set; }
    public string Name { get; private set; }

    /// <summary>
    /// Default constructor for ORM compatibility.
    /// </summary>
    protected Permission() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission"/> class.
    /// </summary>
    /// <param name="permissionId">The unique identifier for the permission.</param>
    /// <param name="name">The permission name.</param>
    /// <exception cref="ArgumentException">Thrown when any string parameter is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when any object parameter is null.</exception>
    public static Permission Create(Guid permissionId, string name)
    {
        return new Permission(new PermissionId(permissionId), name);
    }    
    
    private Permission(PermissionId permissionId, string name)
    {
        ValidatePermissionDetails(name);

        Id = permissionId;
        Name = name;
    }

    public void ChangeName(string name)
    {
        ValidatePermissionDetails(name);
        Name = name;
    }
    
    /// <summary>
    /// Validates permission details.
    /// </summary>
    private static void ValidatePermissionDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name can't be null or empty.", nameof(name));
    }
}