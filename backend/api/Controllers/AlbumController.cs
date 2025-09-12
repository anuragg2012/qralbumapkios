using Microsoft.AspNetCore.Authorization;
using QRAlbums.API.Models;
using QRAlbums.API.Services;
using System.Security.Claims;

namespace QRAlbums.API.Controllers;

public static class AlbumController
{
    public static void MapAlbumEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("").WithTags("Albums").RequireAuthorization();

        // Create album under project
        group.MapPost("/projects/{projectId:long}/albums", async (long projectId, CreateAlbumRequest request, IAlbumService albumService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var result = await albumService.CreateAlbumAsync(userId, projectId, request);
            return Results.Created($"/albums/{result.Id}", result);
        });

        // Get album details (owner view)
        group.MapGet("/albums/{id:long}", async (long id, IAlbumService albumService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var album = await albumService.GetAlbumDetailAsync(userId, id);
            return album != null ? Results.Ok(album) : Results.NotFound();
        });

        // Get selection summary
        group.MapGet("/albums/{id:long}/selections/summary", async (long id, IAlbumService albumService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var summary = await albumService.GetSelectionSummaryAsync(userId, id);
            return Results.Ok(summary);
        });

        // Finalize album
        group.MapPost("/albums/{id:long}/finalize", async (long id, FinalizeAlbumRequest request, IAlbumService albumService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var result = await albumService.FinalizeAlbumAsync(userId, id, request);
            return Results.Ok(result);
        });
    }

    private static long GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim!);
    }
}