using Microsoft.EntityFrameworkCore;
using QRAlbums.API.Data;
using QRAlbums.API.Models;

namespace QRAlbums.API.Services;

public class MediaService : IMediaService
{
    private readonly QRAlbumsContext _context;
    private readonly IProjectService _projectService;
    private readonly IBunnyService _bunnyService;

    public MediaService(QRAlbumsContext context, IProjectService projectService, IBunnyService bunnyService)
    {
        _context = context;
        _projectService = projectService;
        _bunnyService = bunnyService;
    }

    public async Task<UploadResponse> UploadItemAsync(long albumId, IFormFile file, ItemKind kind, bool watermarkEnabled = false, string? watermarkText = null)
    {
        // Get album with project info
        var album = await _context.Albums
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == albumId);

        if (album == null) throw new ArgumentException("Album not found");

        // Assign serial number
        var serialNo = await _projectService.AssignNextSerialAsync(album.ProjectId);

        // Generate file paths
        var guid = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var basePath = $"u/{album.Project.OwnerId}/p/{album.ProjectId}/a/{albumId}";

        string srcUrl;
        string? wmUrl = null;
        string? thumbUrl = null;
        int? width = null;
        int? height = null;

        if (kind == ItemKind.IMAGE)
        {
            var originalPath = $"{basePath}/original/{guid}{extension}";
            using var upload = file.OpenReadStream();
            var ok = await _bunnyService.PutAsync(originalPath, upload, file.ContentType);
            if (!ok) throw new InvalidOperationException("Bunny upload failed");
            srcUrl = _bunnyService.BuildCdnUrl(originalPath);
        }
        else if (kind == ItemKind.VIDEO)
        {
            var videoPath = $"{basePath}/video/{guid}{extension}";
            using var upload = file.OpenReadStream();
            var ok = await _bunnyService.PutAsync(videoPath, upload, file.ContentType);
            if (!ok) throw new InvalidOperationException("Bunny upload failed");
            srcUrl = _bunnyService.BuildCdnUrl(videoPath);
        }
        else
        {
            throw new ArgumentException("Unsupported file kind");
        }

        // Create album item
        var item = new AlbumItem
        {
            ProjectId = album.ProjectId,
            AlbumId = albumId,
            SerialNo = serialNo,
            Kind = kind,
            SrcUrl = srcUrl,
            WmUrl = wmUrl,
            ThumbUrl = thumbUrl,
            Width = width,
            Height = height,
            Bytes = file.Length,
            SortOrder = 0
        };

        _context.AlbumItems.Add(item);
        await _context.SaveChangesAsync();

        return new UploadResponse(item.Id, item.ProjectId, item.AlbumId, item.SerialNo, srcUrl, wmUrl, thumbUrl);
    }

    public async Task<bool> DeleteItemAsync(long albumId, long itemId)
    {
        var item = await _context.AlbumItems.FirstOrDefaultAsync(i => i.Id == itemId && i.AlbumId == albumId);
        if (item == null) return false;

        _context.AlbumItems.Remove(item);
        await _context.SaveChangesAsync();

        return true;
    }
}