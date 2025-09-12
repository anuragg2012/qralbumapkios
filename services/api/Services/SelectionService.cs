using Microsoft.EntityFrameworkCore;
using QRAlbums.API.Data;
using QRAlbums.API.Models;

namespace QRAlbums.API.Services;

public class SelectionService : ISelectionService
{
    private readonly QRAlbumsContext _context;

    public SelectionService(QRAlbumsContext context)
    {
        _context = context;
    }

    public async Task<ViewerAlbumDto?> GetViewerAlbumAsync(string slug)
    {
        var album = await _context.Albums
            .Where(a => a.Slug == slug && a.Status == AlbumStatus.ACTIVE)
            .Include(a => a.Items)
            .FirstOrDefaultAsync();

        if (album == null) return null;

        var items = album.Items.Select(i =>
        {
            // For RAW albums, prefer watermarked version if available
            string displayUrl = i.SrcUrl;
            if (album.Version == AlbumVersion.RAW && !string.IsNullOrEmpty(i.WmUrl))
            {
                displayUrl = i.WmUrl;
            }

            return new ViewerItemDto(i.Id, i.SerialNo, i.Kind, displayUrl, i.ThumbUrl, i.Width, i.Height);
        }).ToList();

        return new ViewerAlbumDto(album.Title, album.Version, album.AllowSelection, album.SelectionLimit, album.ProjectId, items);
    }

    public async Task<CreateSessionResponse> CreateSessionAsync(string slug)
    {
        var album = await _context.Albums.FirstOrDefaultAsync(a => a.Slug == slug);
        if (album == null) throw new ArgumentException("Album not found");

        var sessionKey = GenerateSessionKey();
        var session = new ViewerSession
        {
            AlbumId = album.Id,
            SessionKey = sessionKey
        };

        _context.ViewerSessions.Add(session);
        await _context.SaveChangesAsync();

        return new CreateSessionResponse(sessionKey);
    }

    public async Task<SubmitSelectionsResponse> SubmitSelectionsAsync(string slug, SubmitSelectionsRequest request)
    {
        var album = await _context.Albums.FirstOrDefaultAsync(a => a.Slug == slug && a.AllowSelection);
        if (album == null) return new SubmitSelectionsResponse(false, "Album not found or selections not allowed");

        // Check session
        var session = await _context.ViewerSessions.FirstOrDefaultAsync(s => s.SessionKey == request.SessionKey && s.AlbumId == album.Id);
        if (session == null) return new SubmitSelectionsResponse(false, "Invalid session");

        if (session.Submitted) return new SubmitSelectionsResponse(false, "Selections already submitted for this session");

        // Validate selection limit
        if (album.SelectionLimit > 0 && request.ItemIds.Count > album.SelectionLimit)
        {
            return new SubmitSelectionsResponse(false, $"Too many selections. Limit is {album.SelectionLimit}");
        }

        // Validate item IDs belong to this album
        var validItemIds = await _context.AlbumItems
            .Where(i => i.AlbumId == album.Id && request.ItemIds.Contains(i.Id))
            .Select(i => i.Id)
            .ToListAsync();

        if (validItemIds.Count != request.ItemIds.Count)
        {
            return new SubmitSelectionsResponse(false, "Some selected items are invalid");
        }

        // Save selections
        var selections = request.ItemIds.Select(itemId => new Selection
        {
            AlbumId = album.Id,
            SessionKey = request.SessionKey,
            ItemId = itemId
        }).ToList();

        _context.Selections.AddRange(selections);

        // Mark session as submitted
        session.Submitted = true;

        await _context.SaveChangesAsync();

        return new SubmitSelectionsResponse(true);
    }

    private static string GenerateSessionKey()
    {
        var bytes = new byte[16];
        Random.Shared.NextBytes(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=');
    }
}