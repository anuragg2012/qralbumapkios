using Microsoft.EntityFrameworkCore;
using QRAlbums.API.Data;
using QRAlbums.API.Models;

namespace QRAlbums.API.Services;

public class ProjectService : IProjectService
{
    private readonly QRAlbumsContext _context;

    public ProjectService(QRAlbumsContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> CreateProjectAsync(long userId, CreateProjectRequest request)
    {
        var project = new Project
        {
            OwnerId = userId,
            Name = request.Name,
            Key = GenerateProjectKey()
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Initialize counter after project ID is generated
        _context.ProjectCounters.Add(new ProjectCounter { ProjectId = project.Id });
        await _context.SaveChangesAsync();

        return new ProjectDto(project.Id, project.Name, project.Key, project.CreatedAt, 0);
    }

    public async Task<ProjectDto?> UpdateProjectAsync(long userId, long projectId, UpdateProjectRequest request)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
        if (project == null) return null;

        project.Name = request.Name;
        await _context.SaveChangesAsync();

        return new ProjectDto(project.Id, project.Name, project.Key, project.CreatedAt, project.Albums.Count);
    }

    public async Task<List<ProjectDto>> GetUserProjectsAsync(long userId)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == userId)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Key,
                p.CreatedAt,
                p.Albums.Count
            ))
            .ToListAsync();
    }

    public async Task<ProjectDetailDto?> GetProjectDetailAsync(long userId, long projectId)
    {
        var project = await _context.Projects
            .Where(p => p.Id == projectId && p.OwnerId == userId)
            .Include(p => p.Albums)
            .FirstOrDefaultAsync();

        if (project == null) return null;

        var albums = project.Albums.Select(a => new AlbumSummaryDto(
            a.Id,
            a.Slug,
            a.Title,
            a.Version,
            a.Status,
            a.Items.Count,
            a.CreatedAt
        )).ToList();

        return new ProjectDetailDto(project.Id, project.Name, project.Key, project.CreatedAt, albums);
    }

    public async Task<long> AssignNextSerialAsync(long projectId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get or create counter with lock
            var counter = await _context.ProjectCounters
                .Where(pc => pc.ProjectId == projectId)
                .ExecuteUpdateAsync(pc => pc.SetProperty(p => p.LastSerial, p => p.LastSerial + 1));

            if (counter == 0)
            {
                // Counter doesn't exist, create it
                _context.ProjectCounters.Add(new ProjectCounter { ProjectId = projectId, LastSerial = 1 });
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return 1L;
            }

            // Get the updated value
            var updatedCounter = await _context.ProjectCounters
                .Where(pc => pc.ProjectId == projectId)
                .FirstAsync();

            await transaction.CommitAsync();
            return updatedCounter.LastSerial;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string GenerateProjectKey()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}