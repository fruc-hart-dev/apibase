using ApiBase.Models;
using ApiBase.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBase.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async ([FromBody] LoginRequest request, IAuthService authService) =>
        {
            var response = await authService.AuthenticateAsync(request);
            
            if (response == null)
                return Results.Unauthorized();
            
            return Results.Ok(response);
        })
        .WithName("Login")
        .AllowAnonymous()
        .WithOpenApi();

        app.MapPost("/auth/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService authService) =>
        {
            var response = await authService.RefreshTokenAsync(request.RefreshToken);
            
            if (response == null)
                return Results.Unauthorized();
            
            return Results.Ok(response);
        })
        .WithName("RefreshToken")
        .AllowAnonymous()
        .WithOpenApi();

        app.MapPost("/auth/revoke", async ([FromBody] RevokeTokenRequest request, IAuthService authService) =>
        {
            var result = await authService.RevokeTokenAsync(request.RefreshToken);
            
            if (!result)
                return Results.BadRequest();
            
            return Results.Ok();
        })
        .WithName("RevokeToken")
        .WithOpenApi();

        return app;
    }
}