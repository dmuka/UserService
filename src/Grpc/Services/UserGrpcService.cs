using Application.Users.SignIn;
using Application.Users.SignInByToken;
using Application.Users.SignUp;
using Core;
using Grpc.Core;
using Grpc.Protos;
using MediatR;
using Microsoft.Extensions.Logging;
using SignInResponse = Grpc.Protos.SignInResponse;

namespace Grpc.Services;

public class UserGrpcService(ILogger<UserGrpcService> logger, ISender sender) : UserService.UserServiceBase
{
    public override async Task<SignUpResponse> SignUp(SignUpRequest request, ServerCallContext context) 
    { 
        try 
        {
            var command = new SignUpUserCommand(
                request.Username,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Password);

            var result = await sender.Send(command, context.CancellationToken);
             
            return result.IsSuccess 
                ? new SignUpResponse { Id = result.Value.ToString() } 
                : throw new RpcException(new Status(GetStatusCode(result.Error.Type), result.Error.Description)); 
        } 
        catch (Exception ex) 
        { 
            logger.LogError(ex, "Error registering user"); 
            throw new RpcException(new Status(StatusCode.Internal, ex.Message)); 
        } 
    } 

    public override async Task<SignInResponse> SignIn(SignInRequest request, ServerCallContext context) 
    { 
        try 
        { 
            var command = new SignInUserCommand(
                request.Username,
                request.Password,
                request.Email);

            var result = await sender.Send(command, context.CancellationToken);
             
            return result.IsSuccess 
                ? new SignInResponse 
                {
                    AccessToken = result.Value.AccessToken, 
                    RefreshToken = result.Value.RefreshToken
                } 
                : throw new RpcException(new Status(GetStatusCode(result.Error.Type), result.Error.Description));
        } 
        catch (Exception ex) { 
            logger.LogError(ex, "Error authenticating user"); 
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message)); 
        } 
    } 

    public override async Task<SignInResponse> SignInByToken(SignInByTokenRequest request, ServerCallContext context) 
    { 
        try 
        { 
            var command = new SignInUserByTokenCommand(request.RefreshToken);
            
            var result = await sender.Send(command, context.CancellationToken);
             
            return result.IsSuccess 
                ? new SignInResponse
                {
                    AccessToken = result.Value.AccessToken, 
                    RefreshToken = result.Value.RefreshToken
                } 
                : throw new RpcException(new Status(GetStatusCode(result.Error.Type), result.Error.Description));
        } 
        catch (Exception ex) 
        { 
            logger.LogError(ex, "Error refreshing token"); 
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message)); 
        } 
    }

    private StatusCode GetStatusCode(ErrorType errorType)
    {
        var code = errorType switch
        {
            ErrorType.Conflict => StatusCode.AlreadyExists,
            ErrorType.NotFound => StatusCode.NotFound,
            ErrorType.Failure => StatusCode.Internal,
            ErrorType.Problem => StatusCode.Internal,
            ErrorType.Validation => StatusCode.InvalidArgument,
            _ => StatusCode.Unknown
        };
        
        return code;
    }
}