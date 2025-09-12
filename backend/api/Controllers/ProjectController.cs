using Microsoft.AspNetCore.Authorization;
using QRAlbums.API.Models;
using QRAlbums.API.Services;
using System.Security.Claims;

namespace QRAlbums.API.Controllers;

public static class ProjectController
{
    public static void MapProjectEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/projects").WithTags("Projects").RequireAuthorization();

        group.MapPost("", async (CreateProjectRequest request, IProjectService projectService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var result = await projectService.CreateProjectAsync(userId, request);
            return Results.Created($"/projects/{result.Id}", result);
        });

        group.MapGet("", async (IProjectService projectService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var projects = await projectService.GetUserProjectsAsync(userId);
            return Results.Ok(projects);
        });

        group.MapGet("{id:long}", async (long id, IProjectService projectService, HttpContext context) =>
        {
            var userId = GetUserId(context);
            var project = await projectService.GetProjectDetailAsync(userId, id);
            return project != null ? Results.Ok(project) : Results.NotFound();
        });
    }

    private static long GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim!);
    }
}