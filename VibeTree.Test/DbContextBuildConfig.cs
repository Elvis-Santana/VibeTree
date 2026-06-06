using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Infrastructure.AppDbContext;

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

    }

    public async Task<IWriteDbContext> CriarContextoWritePreparadoAsync()
    {
        var context = new WriteDbContext(_writeOptions);

        await  context.Database.EnsureCreatedAsync();

        return context;
    }
    public async Task<IReadDbContext> CriarContextoReadPreparadoAsync()
    {
        var context = new ReadDbContext(_readOptions);
        await context.Database.EnsureCreatedAsync();
        return context;
    }


    public async ValueTask DisposeAsync()
    {
        await _sqliteConnection.CloseAsync();
        await _sqliteConnection.DisposeAsync();
    }
}
