using System.ComponentModel.DataAnnotations;

namespace QRAlbums.API.Models;

public class User
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}

public class Project
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}

public class ProjectCounter
{
    public long ProjectId { get; set; }
    public long LastSerial { get; set; }
}

public enum AlbumVersion
{
    RAW,
    FINAL
}

public enum AlbumStatus
{
    ACTIVE,
    ARCHIVED
}

public class Album
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public long OwnerId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public AlbumVersion Version { get; set; }
    public bool AllowSelection { get; set; }
    public int SelectionLimit { get; set; }
    public AlbumStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<AlbumItem> Items { get; set; } = new List<AlbumItem>();
}

public enum ItemKind
{
    IMAGE,
    VIDEO
}

public class AlbumItem
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public long AlbumId { get; set; }
    public long SerialNo { get; set; }
    public ItemKind Kind { get; set; }
    public string SrcUrl { get; set; } = string.Empty;
    public string? WmUrl { get; set; }
    public string? ThumbUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? Bytes { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public Album Album { get; set; } = null!;
}

public class ViewerSession
{
    public long Id { get; set; }
    public long AlbumId { get; set; }
    public string SessionKey { get; set; } = string.Empty;
    public bool Submitted { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Album Album { get; set; } = null!;
}

public class Selection
{
    public long Id { get; set; }
    public long AlbumId { get; set; }
    public string SessionKey { get; set; } = string.Empty;
    public long ItemId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Album Album { get; set; } = null!;
    public AlbumItem Item { get; set; } = null!;
}