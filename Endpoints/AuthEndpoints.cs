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

        return app;
    }
}