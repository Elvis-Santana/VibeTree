using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Test;

public  class DbContextBuildConfig : IAsyncDisposable
{
    private readonly DbContextOptions<WriteDbContext> _writeOptions;
    private readonly DbContextOptions<ReadDbContext> _readOptions;
    private readonly SqliteConnection _sqliteConnection;

    public DbContextBuildConfig()
    {
         _sqliteConnection = new SqliteConnection("Filename=:memory:");
         _sqliteConnection.Open();

        _writeOptions = new DbContextOptionsBuilder<WriteDbContext>()
         .UseSqlite(_sqliteConnection)
         .Options;

        _readOptions = new DbContextOptionsBuilder<ReadDbContext>()
         .UseSqlite(_sqliteConnection)
         .Options;

        using var setupContext = new WriteDbContext(_writeOptions);
        setupContext.Database.EnsureCreated();
    }

    public async Task<WriteDbContext> CreateWriteContext()=>
         new WriteDbContext(_writeOptions);
       
    public async Task<ReadDbContext> CreateReadContext()=>
        new ReadDbContext(_readOptions);
        
    


    public async ValueTask DisposeAsync()
    {
        await _sqliteConnection.CloseAsync();
        await _sqliteConnection.DisposeAsync();
    }
}
