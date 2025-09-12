using Microsoft.EntityFrameworkCore;
using QRAlbums.API.Data;
using QRAlbums.API.Models;

namespace QRAlbums.API.Services;

public class AlbumService : IAlbumService
{
    private readonly QRAlbumsContext _context;
    private readonly IProjectService _projectService;
    private readonly IQRService _qrService;
    private readonly IConfiguration _config;

    public AlbumService(QRAlbumsContext context, IProjectService projectService, IQRService qrService, IConfiguration config)
    {
        _context = context;
        _projectService = projectService;
        _qrService = qrService;
        _config = config;
    }

    public async Task<AlbumDto> CreateAlbumAsync(long userId, long projectId, CreateAlbumRequest request)
    {
        // Verify project ownership
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
        if (project == null) throw new UnauthorizedAccessException("Project not found or access denied");

        var album = new Album
        {
            ProjectId = projectId,
            OwnerId = userId,
            Slug = GenerateSlug(),
            Title = request.Title,
            Version = AlbumVersion.RAW,
            AllowSelection = true,
            SelectionLimit = request.SelectionLimit
        };

        _context.Albums.Add(album);
        await _context.SaveChangesAsync();

        var shareUrl = $"{_config["Frontend:ViewerBaseUrl"]}/a/{album.Slug}";
        var qrPngBase64 = _qrService.GenerateQRCodeBase64(shareUrl);

        return new AlbumDto(album.Id, album.ProjectId, album.Slug, album.Title, album.Version, album.AllowSelection, album.SelectionLimit, album.Status, album.CreatedAt, shareUrl, qrPngBase64);
    }

    public async Task<AlbumDetailDto?> GetAlbumDetailAsync(long userId, long albumId)
    {
        var album = await _context.Albums
            .Where(a => a.Id == albumId && a.OwnerId == userId)
            .Include(a => a.Items)
            .FirstOrDefaultAsync();

        if (album == null) return null;

        var items = album.Items.Select(i => new AlbumItemDto(
            i.Id, i.ProjectId, i.AlbumId, i.SerialNo, i.Kind, i.SrcUrl, i.WmUrl, i.ThumbUrl, i.Width, i.Height, i.Bytes, i.SortOrder, i.CreatedAt
        )).ToList();

        return new AlbumDetailDto(album.Id, album.ProjectId, album.Slug, album.Title, album.Version, album.AllowSelection, album.SelectionLimit, album.Status, album.CreatedAt, items);
    }

    public async Task<List<SelectionSummaryDto>> GetSelectionSummaryAsync(long userId, long albumId)
    {
        // Verify ownership
        var album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == albumId && a.OwnerId == userId);
        if (album == null) throw new UnauthorizedAccessException("Album not found or access denied");

        var summary = await _context.Selections
            .Where(s => s.AlbumId == albumId)
            .GroupBy(s => new { s.ItemId, s.Item.SerialNo, s.Item.ThumbUrl })
            .Select(g => new SelectionSummaryDto(
                g.Key.ItemId,
                g.Key.SerialNo,
                g.Key.ThumbUrl ?? "",
                g.Count()
            ))
            .OrderByDescending(s => s.PicksCount)
            .ToListAsync();

        return summary;
    }

    public async Task<AlbumDto> FinalizeAlbumAsync(long userId, long albumId, FinalizeAlbumRequest request)
    {
        // Verify ownership and that it's a RAW album
        var rawAlbum = await _context.Albums
            .Where(a => a.Id == albumId && a.OwnerId == userId && a.Version == AlbumVersion.RAW)
            .Include(a => a.Items)
            .FirstOrDefaultAsync();

        if (rawAlbum == null) throw new UnauthorizedAccessException("RAW album not found or access denied");

        // Create FINAL album
        var finalAlbum = new Album
        {
            ProjectId = rawAlbum.ProjectId,
            OwnerId = userId,
            Slug = GenerateSlug(),
            Title = $"{rawAlbum.Title} (Final)",
            Version = AlbumVersion.FINAL,
            AllowSelection = false,
            SelectionLimit = 0
        };

        _context.Albums.Add(finalAlbum);
        await _context.SaveChangesAsync();

        // Clone selected items with new serial numbers
        var selectedItems = rawAlbum.Items.Where(i => request.ItemIds.Contains(i.Id)).ToList();
        
        foreach (var item in selectedItems)
        {
            var newSerial = await _projectService.AssignNextSerialAsync(rawAlbum.ProjectId);
            
            var finalItem = new AlbumItem
            {
                ProjectId = rawAlbum.ProjectId,
                AlbumId = finalAlbum.Id,
                SerialNo = newSerial,
                Kind = item.Kind,
                SrcUrl = item.SrcUrl,
                WmUrl = item.WmUrl,
                ThumbUrl = item.ThumbUrl,
                Width = item.Width,
                Height = item.Height,
                Bytes = item.Bytes,
                SortOrder = item.SortOrder
            };

            _context.AlbumItems.Add(finalItem);
        }

        await _context.SaveChangesAsync();

        var shareUrl = $"{_config["Frontend:ViewerBaseUrl"]}/a/{finalAlbum.Slug}";
        var qrPngBase64 = _qrService.GenerateQRCodeBase64(shareUrl);

        return new AlbumDto(finalAlbum.Id, finalAlbum.ProjectId, finalAlbum.Slug, finalAlbum.Title, finalAlbum.Version, finalAlbum.AllowSelection, finalAlbum.SelectionLimit, finalAlbum.Status, finalAlbum.CreatedAt, shareUrl, qrPngBase64);
    }

    private static string GenerateSlug()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}