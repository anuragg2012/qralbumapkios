using QRAlbums.API.Models;
using QRAlbums.API.Services;

namespace QRAlbums.API.Controllers;

public static class ViewerController
{
    public static void MapViewerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/a").WithTags("Viewer (Public)");

        // Get album for public viewing
        group.MapGet("{slug}", async (string slug, ISelectionService selectionService) =>
        {
            var album = await selectionService.GetViewerAlbumAsync(slug);
            return album != null ? Results.Ok(album) : Results.NotFound();
        });

        // Create viewer session
        group.MapPost("{slug}/sessions", async (string slug, ISelectionService selectionService) =>
        {
            try
            {
                var result = await selectionService.CreateSessionAsync(slug);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
    }
}