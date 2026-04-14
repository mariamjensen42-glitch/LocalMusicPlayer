using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using LocalMusicPlayer.Models;

namespace LocalMusicPlayer.Data;

public class AppDbContext : DbContext
{
    private readonly object _syncLock = new();
    public object SyncLock => _syncLock;

    public DbSet<SongEntity> Songs => Set<SongEntity>();
    public DbSet<FavoriteEntity> Favorites => Set<FavoriteEntity>();
    public DbSet<PlayHistoryEntity> PlayHistory => Set<PlayHistoryEntity>();
    public DbSet<AppSettingsEntity> Settings => Set<AppSettingsEntity>();
    public DbSet<PlaylistEntity> Playlists => Set<PlaylistEntity>();
    public DbSet<PlaylistSongEntity> PlaylistSongs => Set<PlaylistSongEntity>();
    public DbSet<SongStatisticsEntity> SongStatistics => Set<SongStatisticsEntity>();
    public DbSet<ListeningHistoryRecordEntity> ListeningHistory => Set<ListeningHistoryRecordEntity>();
    public DbSet<StatisticsMetaEntity> StatisticsMeta => Set<StatisticsMetaEntity>();

    private readonly string _dbPath;

    public AppDbContext()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbDirectory = Path.Combine(appDataPath, "LocalMusicPlayer");
        Directory.CreateDirectory(dbDirectory);
        _dbPath = Path.Combine(dbDirectory, "musicplayer.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SongEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.HasIndex(e => e.Artist);
            entity.HasIndex(e => e.Album);
            entity.HasIndex(e => e.AlbumArtPath);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Artist).HasMaxLength(500);
            entity.Property(e => e.Album).HasMaxLength(500);
            entity.Property(e => e.Genre).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(2000);
            entity.Property(e => e.AlbumArtPath).HasMaxLength(2000);
        });

        modelBuilder.Entity<FavoriteEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.Property(e => e.FilePath).HasMaxLength(2000);
        });

        modelBuilder.Entity<PlayHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PlayedAt);
            entity.HasIndex(e => e.FilePath);
            entity.Property(e => e.FilePath).HasMaxLength(2000);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Artist).HasMaxLength(500);
        });

        modelBuilder.Entity<AppSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Value).HasMaxLength(4000);
        });

        modelBuilder.Entity<PlaylistEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlaylistId).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.PlaylistId).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CoverArtPath).HasMaxLength(2000);
        });

        modelBuilder.Entity<PlaylistSongEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlaylistId, e.FilePath }).IsUnique();
            entity.Property(e => e.PlaylistId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<SongStatisticsEntity>(entity =>
        {
            entity.HasKey(e => e.FilePath);
            entity.Property(e => e.FilePath).HasMaxLength(2000);
        });

        modelBuilder.Entity<ListeningHistoryRecordEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PlayedAt);
            entity.HasIndex(e => e.FilePath);
            entity.Property(e => e.FilePath).HasMaxLength(2000);
        });

        modelBuilder.Entity<StatisticsMetaEntity>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(100);
        });
    }
}

public class SongEntity
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Year { get; set; }
    public int TrackNumber { get; set; }
    public TimeSpan Duration { get; set; }
    public int PlayCount { get; set; }
    public DateTime? LastPlayedAt { get; set; }
    public string AlbumArtPath { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.Now;
}

public class FavoriteEntity
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.Now;
}

public class PlayHistoryEntity
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; } = DateTime.Now;
}

public class AppSettingsEntity
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class PlaylistEntity
{
    public int Id { get; set; }
    public string PlaylistId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    public DateTime? ModifiedTime { get; set; }
    public string? CoverArtPath { get; set; }
}

public class PlaylistSongEntity
{
    public int Id { get; set; }
    public string PlaylistId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class SongStatisticsEntity
{
    public string FilePath { get; set; } = string.Empty;
    public int PlayCount { get; set; }
    public DateTime? LastPlayedTime { get; set; }
    public long TotalPlayedDurationMs { get; set; }
}

public class ListeningHistoryRecordEntity
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
    public long PlayedDurationMs { get; set; }
    public double CompletionPercentage { get; set; }
}

public class StatisticsMetaEntity
{
    public string Key { get; set; } = string.Empty;
    public long TotalPlayTimeMs { get; set; }
    public DateTime? FirstScanDate { get; set; }
}
