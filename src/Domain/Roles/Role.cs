using Core;
using Domain.Roles.DomainEvents;
using Domain.Roles.Specifications;
using Domain.Users;

namespace Domain.Roles;

public class Role : Entity<RoleId>, IAggregationRoot
{
    public string Name { get; private set; } = string.Empty;
    public ICollection<UserId> UserIds { get; private set; } = [];

    protected Role() { }

    public static Result<Role> Create(Guid roleId, string roleName)
    {
        var resultsWithFailures = ValidateRoleDetails(roleName);

        if (resultsWithFailures.Length != 0)
        {
            return Result<Role>.ValidationFailure(ValidationError.FromResults(resultsWithFailures));
        }

        var role = new Role(new RoleId(roleId), roleName);
        
        var roleCreatedEvent = new RoleCreatedDomainEvent(roleId);
        role.AddDomainEvent(roleCreatedEvent);
        
        return role;
    }

    private Role(RoleId id, string name)
    {
        Id = id;
        Name = name;
        UserIds = new List<UserId>();
    }
        
    public void AddUser(UserId userId)
    {
        UserIds.Add(userId);
    }
    
    /// <summary>
    /// Validates role details.
    /// </summary>
    private static Result[] ValidateRoleDetails(string roleName)
    {
        var validationResults = new []
        {
            new RoleNameMustBeValid(roleName).IsSatisfied()
        };
            
        var results = validationResults.Where(result => result.IsFailure);

        return results.ToArray();
    }
}