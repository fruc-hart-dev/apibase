using System.Security.Cryptography;
using System.Text;
using ApiBase.Models;
using ApiBase.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBase.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async ([FromBody] RegisterRequest request, IUserService userService) =>
        {
            // Check if user already exists
            var existingUser = await userService.GetUserByNameAsync(request.Username);
            if (existingUser != null)
                return Results.BadRequest("Username already exists");

            // Hash the password
            string hashedPassword = HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Name = request.Username,
                Password = hashedPassword
            };

            await userService.CreateUserAsync(user);
            return Results.Ok(new { message = "User registered successfully" });
        })
        .WithName("Register")
        .AllowAnonymous()
        .WithOpenApi();

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

        app.MapPost("/auth/revoke", async ([FromBody] RevokeTokenRequest request, IAuthService authService) =>
        {
            var result = await authService.RevokeTokenAsync(request.RefreshToken);
            if (!result)
                return Results.BadRequest("Invalid token");
            
            return Results.Ok(new { message = "Token revoked successfully" });
        })
        .WithName("RevokeToken")
        .WithOpenApi();

        app.MapPost("/auth/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService authService) =>
        {
            var response = await authService.RefreshTokenAsync(request.RefreshToken);
            if (response == null)
                return Results.BadRequest("Invalid token");
            
            return Results.Ok(response);
        })
        .WithName("RefreshToken")
        .AllowAnonymous()
        .WithOpenApi();

        return app;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}