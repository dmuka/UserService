using Core;
using MediatR;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Interface for handling queries that return a specific type of response wrapped in a <see cref="Result{TResponse}"/>.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
