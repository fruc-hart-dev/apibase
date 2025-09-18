using ApiBase.Models;
using ApiBase.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBase.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Get all users
        app.MapGet("/users", async (IUserService userService) =>
        {
            var users = await userService.GetAllUsersAsync();
            return Results.Ok(users);
        })
        .WithName("GetAllUsers")
        .RequireAuthorization()
        .WithOpenApi();

        // Get user by ID
        app.MapGet("/users/{id}", async (Guid id, IUserService userService) =>
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
                return Results.NotFound();
            
            return Results.Ok(user);
        })
        .WithName("GetUserById")
        .RequireAuthorization()
        .WithOpenApi();

        // Create user (Register) - No authentication required
        app.MapPost("/users/register", async ([FromBody] User user, IUserService userService) =>
        {
            var createdUser = await userService.CreateUserAsync(user);
            return Results.Created($"/users/{createdUser.Id}", createdUser);
        })
        .WithName("RegisterUser")
        .AllowAnonymous()
        .WithOpenApi();

        // Update user
        app.MapPut("/users/{id}", async (Guid id, [FromBody] User user, IUserService userService) =>
        {
            var updatedUser = await userService.UpdateUserAsync(id, user);
            if (updatedUser == null)
                return Results.NotFound();
            
            return Results.Ok(updatedUser);
        })
        .WithName("UpdateUser")
        .RequireAuthorization()
        .WithOpenApi();

        // Delete user
        app.MapDelete("/users/{id}", async (Guid id, IUserService userService) =>
        {
            var deleted = await userService.DeleteUserAsync(id);
            if (!deleted)
                return Results.NotFound();
            
            return Results.NoContent();
        })
        .WithName("DeleteUser")
        .RequireAuthorization()
        .WithOpenApi();

        return app;
    }
}