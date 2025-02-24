using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Grpc.Infrastructure.Interceptors;

public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Received request {Request}", request);
        var response = await continuation(request, context);
        logger.LogInformation("Finished handling gRPC {Request}", request);
        
        return response;
    }
}