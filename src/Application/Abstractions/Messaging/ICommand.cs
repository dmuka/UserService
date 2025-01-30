using Core;
using MediatR;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for all commands.
/// </summary>
public interface IBaseCommand;

/// <summary>
/// Represents a command that returns a generic <see cref="Result"/> indicating success or failure.
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>
/// Represents a command that returns a specific type of response wrapped in a <see cref="Result{TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;
