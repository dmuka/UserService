namespace WebApi;

/// <summary>
/// Represents an endpoint in the web API.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified route builder.
    /// </summary>
    /// <param name="builder">The route builder used to map the endpoint.</param>
    void MapEndpoint(IEndpointRouteBuilder builder);
}
