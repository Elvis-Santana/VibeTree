using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Test;

public  class DbContextBuildConfig : IAsyncDisposable
{
    private readonly DbContextOptions<WriteDbContext> _writeOptions;
    private readonly DbContextOptions<ReadDbContext> _readOptions;
    private readonly SqliteConnection _writeConnection;
    private readonly SqliteConnection _readConnection;
    public DbContextBuildConfig()
    {


        _writeConnection = new SqliteConnection($"Data Source=WriteDb_{Guid.NewGuid().ToString()};Mode=Memory;Cache=Shared");
        _writeConnection.Open();

        _writeOptions = new DbContextOptionsBuilder<WriteDbContext>()
            .UseSqlite(_writeConnection)
            .Options;


        using var setupWriteContext = new WriteDbContext(_writeOptions);
        setupWriteContext.Database.EnsureCreated();

        _readConnection = new SqliteConnection($"Data Source=ReadDb_{Guid.NewGuid().ToString()};Mode=Memory;Cache=Shared");
        _readConnection.Open();

        _readOptions = new DbContextOptionsBuilder<ReadDbContext>()
            .UseSqlite(_readConnection)
            .Options;

        using var setupReadContext = new ReadDbContext(_readOptions);
        setupReadContext.Database.EnsureCreated();

    }

    public async Task<WriteDbContext> CreateWriteContext()=>
         new WriteDbContext(_writeOptions);
       
    public async Task<ReadDbContext> CreateReadContext()=>
        new ReadDbContext(_readOptions);
        
    


    public async ValueTask DisposeAsync()
    {
        await _readConnection.CloseAsync();
        await _readConnection.DisposeAsync();

        await _writeConnection.CloseAsync();
        await _writeConnection.DisposeAsync();

    }
}
