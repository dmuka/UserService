using Core;
using MediatR;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Interface for handling commands that return a generic <see cref="Result"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Interface for handling commands that return a specific type of response wrapped in a <see cref="Result{TResponse}"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
