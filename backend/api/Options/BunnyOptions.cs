namespace QRAlbums.API.Options;

public sealed class BunnyOptions
{
    public string StorageZone { get; set; } = "";
    public string AccessKey   { get; set; } = "";
    public string CdnBase     { get; set; } = "";
    public string? StreamLibraryId { get; set; }
}

