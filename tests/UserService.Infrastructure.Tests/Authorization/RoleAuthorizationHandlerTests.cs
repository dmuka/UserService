using System.Security.Claims;
using Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace UserService.Infrastructure.Tests.Authorization;

[TestFixture]
public class RoleAuthorizationHandlerTests
{
    private const string Admin = "Admin";
    private const string User = "User";
    private const string SuperAdmin = "SuperAdmin";
    
    private const string MockAuthType = "MockAuthType";
    
    private ClaimsPrincipal _adminPrincipalMock;
    private RolesAuthorizationRequirement[] _adminAuthorizationRequirements;
    private RolesAuthorizationRequirement[] _multipleRolesAuthorizationRequirements;
    private RolesAuthorizationRequirement[] _emptyAuthorizationRequirements;
    
    private RoleAuthorizationHandler _handler;

    [SetUp]
    public void Setup()
    {
        _adminPrincipalMock = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, Admin)
        ], MockAuthType));
        
        _adminAuthorizationRequirements = [new RolesAuthorizationRequirement([Admin])];
        _multipleRolesAuthorizationRequirements = [new RolesAuthorizationRequirement([Admin, SuperAdmin])];
        _emptyAuthorizationRequirements = [];
        
        _handler = new RoleAuthorizationHandler();
    }
    
    [TestCase(true, Admin, Admin, Description = "User has required role")]
    [TestCase(false, User, Admin, Description = "User does not have required role")]
    [TestCase(false, null, Admin, Description = "User is not authenticated")]
    public async Task HandleRequirementAsync_Authorization_ShouldBehaveCorrectly(
        bool expectedSuccess, 
        string? userRole, 
        string? requiredRole)
    {
        // Arrange
        var principal = userRole == null 
            ? new ClaimsPrincipal(new ClaimsIdentity()) 
            : new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, userRole)], MockAuthType));
        var context = new AuthorizationHandlerContext(_adminAuthorizationRequirements, principal, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.That(context.HasSucceeded, Is.EqualTo(expectedSuccess));
    }
    
    [TestCase(Admin, Description = "User has required role")]
    [TestCase(User, Description = "User does not have required role")]
    [TestCase(null, Description = "User is not authenticated")]
    public async Task HandleRequirementAsync_AuthorizationWithEmptyRequirements_ShouldBehaveCorrectly(string? userRole)
    {
        // Arrange
        var principal = userRole == null 
            ? new ClaimsPrincipal(new ClaimsIdentity()) 
            : new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, userRole)], MockAuthType));
        var context = new AuthorizationHandlerContext(_emptyAuthorizationRequirements, principal, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.That(context.HasSucceeded, Is.False);
    }
    
    [Test]
    public async Task HandleRequirementAsync_UserHasOneOfMultipleRequiredRoles_ShouldSucceed()
    {
        // Arrange
        var context = new AuthorizationHandlerContext(_multipleRolesAuthorizationRequirements, _adminPrincipalMock, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task HandleRequirementAsync_UserHasMultipleRoles_ShouldSucceed()
    {
        // Arrange
        var multiRolePrincipal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, User),
            new Claim(ClaimTypes.Role, Admin)
        ], MockAuthType));
    
        var context = new AuthorizationHandlerContext(_adminAuthorizationRequirements, multiRolePrincipal, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task HandleRequirementAsync_UserIsNotAuthenticated_ShouldNotSucceed()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var context = new AuthorizationHandlerContext(_adminAuthorizationRequirements, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        Assert.That(context.HasSucceeded, Is.False);
    }
}