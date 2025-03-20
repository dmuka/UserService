using Application.Abstractions.Messaging;

namespace Application.Roles.Add;

public sealed record AddRoleCommand(string Name) : ICommand<Guid>;