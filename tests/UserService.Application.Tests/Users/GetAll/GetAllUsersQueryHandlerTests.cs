﻿using Application.Abstractions.Authentication;
using Application.Users.GetAll;
using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.Emails;
using Domain.ValueObjects.PasswordHashes;
using Domain.ValueObjects.RoleNames;
using Moq;

namespace UserService.Application.Tests.Users.GetAll;

[TestFixture]
public class GetAllUsersQueryHandlerTests
{
    private static readonly RoleName AdminRoleName = RoleName.Create("Admin");
    
    private readonly User _user = 
        User.Create(
            Guid.CreateVersion7(), 
            "name", 
            "First Name", 
            "Last Name", 
            PasswordHash.Create("hash").Value, 
            Email.Create("email@email.com").Value, 
            new List<RoleName> { AdminRoleName }, 
            new List<UserPermissionId>(),
            ["recoveryCode"], 
            false,
            "MfaSecret").Value;
    
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IRoleRepository> _roleRepositoryMock;
    private Mock<IUserContext> _userContextMock;
    private GetAllUsersQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object, _userContextMock.Object);
    }

    [Test]
    public async Task Handle_UserIsNotAuthorized_ReturnsUnauthorizedResult()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserRole).Returns("User");

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.Unauthorized()));
        }
    }

    [Test]
    public async Task Handle_UserIsAuthorized_ReturnsUsers()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserRole).Returns("Admin");
        var users = new List<User> { _user, _user };
        _userRepositoryMock.Setup(x => x.GetAllUsersAsync(_cancellationToken))
            .ReturnsAsync(users);
        _roleRepositoryMock.Setup(x => x.GetRolesByUserIdAsync(_user.Id.Value, _cancellationToken))
            .ReturnsAsync(new List<Role> { Role.Create(Guid.CreateVersion7(), "Admin").Value });

        // Act
        var result = await _handler.Handle(new GetAllUsersQuery(), _cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Count.EqualTo(users.Count));
        }
    }
}