using Microsoft.EntityFrameworkCore;
using QRAlbums.API.Models;

namespace QRAlbums.API.Data;

public class QRAlbumsContext : DbContext
{
    public QRAlbumsContext(DbContextOptions<QRAlbumsContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectCounter> ProjectCounters { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<AlbumItem> AlbumItems { get; set; }
    public DbSet<ViewerSession> ViewerSessions { get; set; }
    public DbSet<Selection> Selections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(160);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(24);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.Owner)
                .WithMany(e => e.Projects)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectCounter
        modelBuilder.Entity<ProjectCounter>(entity =>
        {
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.LastSerial).HasDefaultValue(0L);
            
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Album
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(16);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(160);
            entity.Property(e => e.Version).HasDefaultValue(AlbumVersion.RAW);
            entity.Property(e => e.AllowSelection).HasDefaultValue(true);
            entity.Property(e => e.SelectionLimit).HasDefaultValue(0);
            entity.Property(e => e.Status).HasDefaultValue(AlbumStatus.ACTIVE);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.Project)
                .WithMany(e => e.Albums)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Owner)
                .WithMany(e => e.Albums)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AlbumItem
        modelBuilder.Entity<AlbumItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SerialNo).IsRequired();
            entity.Property(e => e.Kind).IsRequired();
            entity.Property(e => e.SrcUrl).IsRequired();
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => new { e.ProjectId, e.SerialNo }).IsUnique();
            
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Album)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ViewerSession
        modelBuilder.Entity<ViewerSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SessionKey).IsRequired().HasMaxLength(22);
            entity.HasIndex(e => e.SessionKey).IsUnique();
            entity.Property(e => e.Submitted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.Album)
                .WithMany()
                .HasForeignKey(e => e.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Selection
        modelBuilder.Entity<Selection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SessionKey).IsRequired().HasMaxLength(22);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => new { e.AlbumId, e.SessionKey, e.ItemId }).IsUnique();
            
            entity.HasOne(e => e.Album)
                .WithMany()
                .HasForeignKey(e => e.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}