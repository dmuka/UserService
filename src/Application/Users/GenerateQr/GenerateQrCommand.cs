using Application.Abstractions.Messaging;

namespace Application.Users.GenerateQr;

public sealed record GenerateQrCommand(string UserId) : ICommand<string>;