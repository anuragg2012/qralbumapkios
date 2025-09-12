using System.ComponentModel.DataAnnotations;

namespace QRAlbums.API.Models;

// Auth DTOs
public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);

public record UserDto(long Id, string Email, DateTime CreatedAt);

// Project DTOs
public record CreateProjectRequest(string Name);
public record ProjectDto(long Id, string Name, string Key, DateTime CreatedAt, int AlbumCount);
public record ProjectDetailDto(long Id, string Name, string Key, DateTime CreatedAt, List<AlbumSummaryDto> Albums);

// Album DTOs
public record CreateAlbumRequest(string Title, int SelectionLimit = 0);
public record AlbumDto(long Id, long ProjectId, string Slug, string Title, AlbumVersion Version, bool AllowSelection, int SelectionLimit, AlbumStatus Status, DateTime CreatedAt, string ShareUrl, string QrPngBase64);
public record AlbumSummaryDto(long Id, string Slug, string Title, AlbumVersion Version, AlbumStatus Status, int ItemCount, DateTime CreatedAt);
public record AlbumDetailDto(long Id, long ProjectId, string Slug, string Title, AlbumVersion Version, bool AllowSelection, int SelectionLimit, AlbumStatus Status, DateTime CreatedAt, List<AlbumItemDto> Items);

public record FinalizeAlbumRequest(List<long> ItemIds);

// Media DTOs
public record AlbumItemDto(long Id, long ProjectId, long AlbumId, long SerialNo, ItemKind Kind, string SrcUrl, string? WmUrl, string? ThumbUrl, int? Width, int? Height, long? Bytes, int SortOrder, DateTime CreatedAt);

public record UploadResponse(long ItemId, long ProjectId, long AlbumId, long SerialNo, string SrcUrl, string? WmUrl, string? ThumbUrl);

// Viewer DTOs (public)
public record ViewerAlbumDto(string Title, AlbumVersion Version, bool AllowSelection, int SelectionLimit, long ProjectId, List<ViewerItemDto> Items);
public record ViewerItemDto(long Id, long SerialNo, ItemKind Kind, string DisplayUrl, string? ThumbUrl, int? Width, int? Height);

public record CreateSessionResponse(string SessionKey);
public record SubmitSelectionsRequest(string SessionKey, List<long> ItemIds);
public record SubmitSelectionsResponse(bool Success, string? Error = null);

// Selection summary DTOs
public record SelectionSummaryDto(long ItemId, long SerialNo, string ThumbUrl, int PicksCount);