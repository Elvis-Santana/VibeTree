using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Test;

public  class DbContextBuildConfig : IAsyncDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly SqliteConnection _sqliteConnection;

    public DbContextBuildConfig()
    {
         _sqliteConnection = new SqliteConnection("Filename=:memory:");
         _sqliteConnection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
         .UseSqlite(_sqliteConnection)
         .Options;

    }

    public async Task<AppDbContext> CriarContextoPreparadoAsync()
    {
        var context = new AppDbContext(_options);

        await  context.Database.EnsureCreatedAsync();

        return context;
    }


    public async ValueTask DisposeAsync()
    {
        await _sqliteConnection.CloseAsync();
        await _sqliteConnection.DisposeAsync();
    }
}
