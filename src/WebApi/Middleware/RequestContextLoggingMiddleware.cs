using Serilog.Context;

namespace WebApi.Middleware;

/// <summary>
/// Middleware that logs the request context by adding a correlation id to the log context.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <remarks>
/// The correlation id is retrieved from the request headers or generated from the trace identifier
/// if not present. It is then pushed to the log context for tracking purposes.
/// </remarks>
public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "Correlation-Id";
    
    /// <summary>
    /// Invokes the next middleware in the pipeline while adding a "CorrelationId" property
    /// to the logging context, extracted from the HTTP request headers or generated if absent.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            return next.Invoke(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out var correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
