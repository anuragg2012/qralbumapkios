using QRAlbums.API.Models;
using QRAlbums.API.Services;

namespace QRAlbums.API.Controllers;

public static class SelectionController
{
    public static void MapSelectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/a").WithTags("Selections (Public)");

        // Submit selections
        group.MapPost("{slug}/selections", async (string slug, SubmitSelectionsRequest request, ISelectionService selectionService) =>
        {
            var result = await selectionService.SubmitSelectionsAsync(slug, request);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}