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

        var mediaGroup = app.MapGroup("/media").WithTags("Media").RequireAuthorization();

        mediaGroup.MapGet("image", (string filePath, int? w, int? h, string? fit, int? quality, bool wm, string? wmText, IBunnyService bunnyService) =>
        {
            var url = bunnyService.GetImageUrl(filePath, w, h, fit, quality, wm, wmText);
            return Results.Ok(new { url });
        });

        mediaGroup.MapGet("video", (string libraryId, string videoId, IBunnyService bunnyService) =>
        {
            var url = bunnyService.GetVideoUrl(libraryId, videoId);
            return Results.Ok(new { url });
        });
    }
}