using Core;
using MediatR;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that returns a specific type of response wrapped in a <see cref="Result{TResponse}"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
