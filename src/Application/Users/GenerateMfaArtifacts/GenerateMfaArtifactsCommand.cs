using Application.Abstractions.Messaging;

namespace Application.Users.GenerateMfaArtifacts;

public sealed record GenerateMfaArtifactsCommand(string UserId) : ICommand<(string qr, List<string> codes)>;