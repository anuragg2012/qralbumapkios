using Microsoft.AspNetCore.Authorization;
using QRAlbums.API.Models;
using QRAlbums.API.Services;
using System.Security.Claims;

namespace QRAlbums.API.Controllers;

public static class MediaController
{
    public static void MapMediaEndpoints(this WebApplication app)
    {
        var albumGroup = app.MapGroup("/albums").WithTags("Media").RequireAuthorization();

        albumGroup.MapPost("{albumId:long}/items", async (long albumId, IFormFile file, IMediaService mediaService, HttpContext context) =>
        {
            // Parse form data
            var form = await context.Request.ReadFormAsync();
            
            if (!Enum.TryParse<ItemKind>(form["kind"], out var kind))
            {
                return Results.BadRequest("Invalid or missing 'kind' field");
            }

            var watermarkEnabled = bool.TryParse(form["watermarkEnabled"], out var wm) && wm;
            var watermarkText = form["watermarkText"].ToString();

            var result = await mediaService.UploadItemAsync(albumId, file, kind, watermarkEnabled, watermarkText);
            return Results.Created($"/albums/{albumId}/items/{result.ItemId}", result);
        }).DisableAntiforgery();

        albumGroup.MapDelete("{albumId:long}/items/{itemId:long}", async (long albumId, long itemId, IMediaService mediaService) =>
        {
            var success = await mediaService.DeleteItemAsync(albumId, itemId);
            return success ? Results.NoContent() : Results.NotFound();
        });

        var mediaGroup = app.MapGroup("/media").WithTags("Media").RequireAuthorization();

        mediaGroup.MapGet("cdn", (string path, IBunnyService bunnyService) =>
        {
            var url = bunnyService.BuildCdnUrl(path);
            return Results.Ok(new { url });
        });
    }
}