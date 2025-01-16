namespace Domain.Users;

public sealed record UserRegisteredDomainEvent(int UserId) : IDomainEvent;