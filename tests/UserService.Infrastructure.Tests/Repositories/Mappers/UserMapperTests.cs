﻿using Domain.Roles;
using Domain.UserPermissions;
using Domain.Users;
using Domain.ValueObjects.RoleNames;
using Infrastructure.Repositories.Dtos;
using Infrastructure.Repositories.Mappers;
using Moq;

namespace UserService.Infrastructure.Tests.Repositories.Mappers;

[TestFixture]
public class UserMapperTests
{
    private static readonly Guid Id = Guid.CreateVersion7();

    private readonly RoleName _roleName = RoleName.Create("Role");
    
    private const string Username = "jdoe";
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string Email = "jdoe@example.com";
    private const string Hash = "hashedPassword";
    private const bool MfaDisabled = false;
    private const string MfaSecret = "MfaSecret";
    
    private readonly ICollection<string> _recoveryCodes = ["recoveryCode"];
    
    private User _user;
    private IList<RoleName> _roleNames;
    private IList<UserPermissionId> _userPermissionIds;
    
    private Mock<IRoleRepository> _roleRepositoryMock;
    
    private UserMapper _userMapper;

    [SetUp]
    public void Setup()
    {
        _roleNames = [_roleName];
        _userPermissionIds = [new UserPermissionId(Id)];
        
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _roleRepositoryMock.Setup(repository => repository.GetRoleByNameAsync("User", CancellationToken.None))
            .ReturnsAsync(Role.Create(Id, "User").Value);
        
        _user = User.Create(
            Id,
            Username,
            FirstName,
            LastName,
            Hash,
            Email,
            _roleNames,
            _userPermissionIds,
            _recoveryCodes,
            MfaDisabled,
            MfaSecret).Value;
        
        _userMapper = new UserMapper(_roleRepositoryMock.Object);
    }

    [Test]
    public void ToDto_ShouldMapUserToUserDtoCorrectly()
    {
        // Arrange
        // Act
        var userDto = _userMapper.ToDto(_user);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(userDto.Id, Is.EqualTo(Id));
            Assert.That(userDto.Username, Is.EqualTo(Username));
            Assert.That(userDto.FirstName, Is.EqualTo(FirstName));
            Assert.That(userDto.LastName, Is.EqualTo(LastName));
            Assert.That(userDto.PasswordHash, Is.EqualTo(Hash));
            Assert.That(userDto.Email, Is.EqualTo(Email));
        }
    }

    [Test]
    public void ToEntity_ShouldMapUserDtoToUserCorrectly()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Id,
            Username = Username,
            FirstName = FirstName,
            LastName = LastName,
            PasswordHash = Hash,
            Email = Email
        };

        // Act
        var user = _userMapper.ToEntity(userDto);
        user.AddRole(_roleNames[0]);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(user.Id.Value, Is.EqualTo(userDto.Id));
            Assert.That(user.Username, Is.EqualTo(userDto.Username));
            Assert.That(user.FirstName, Is.EqualTo(userDto.FirstName));
            Assert.That(user.LastName, Is.EqualTo(userDto.LastName));
            Assert.That(user.PasswordHash.Value, Is.EqualTo(userDto.PasswordHash));
            Assert.That(user.Email.Value, Is.EqualTo(userDto.Email));
            Assert.That(user.RoleNames, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public void ToEntity_ShouldThrowArgumentNullException_WhenUserDtoIsNull()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _userMapper.ToEntity(null!));
    }
}