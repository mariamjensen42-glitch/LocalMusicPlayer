using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LocalMusicPlayer.Data;

namespace LocalMusicPlayer.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task EnsureDatabaseCreatedAsync();
}

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _dbContext;

    public DatabaseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task InitializeAsync()
    {
        return Task.Run(() =>
        {
            lock (_dbContext.SyncLock)
            {
                _dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
            }
        });
    }

    public Task EnsureDatabaseCreatedAsync()
    {
        return Task.Run(() =>
        {
            lock (_dbContext.SyncLock)
            {
                _dbContext.Database.EnsureCreatedAsync().GetAwaiter().GetResult();
            }
        });
    }
}
