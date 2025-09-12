using Microsoft.AspNetCore.Authorization;
using QRAlbums.API.Models;
using QRAlbums.API.Services;
using System.Security.Claims;

namespace QRAlbums.API.Controllers;

public static class AuthController
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(request);
            return result != null ? Results.Ok(result) : Results.BadRequest("Email already exists");
        });

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            return result != null ? Results.Ok(result) : Results.Unauthorized();
        });

        group.MapGet("/me", async (IAuthService authService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var user = await authService.GetCurrentUserAsync(userId);
            return user != null ? 
                Results.Ok(new UserDto(user.Id, user.Email, user.CreatedAt)) : 
                Results.NotFound();
        }).RequireAuthorization();
    }

    private static long GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim!);
    }
}