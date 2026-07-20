using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.Infrastructure.Query;
using VibeTree.User.CreateUser;

namespace VibeTree.Test;


public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>, IAsyncLifetime
{
    private SqliteConnection? _readConnection;
    private SqliteConnection? _writeConnection;

    private readonly string _uniqueId = Guid.NewGuid().ToString();
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

        await writeDb.Database.EnsureCreatedAsync();
        await readDb.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(async services =>
        {
        //services.RemoveAll(typeof(DbContextOptions<ReadDbContext>));
        //services.RemoveAll(typeof(DbContextOptions<WriteDbContext>));
        //services.RemoveAll(typeof(DbContextOptions));
        //services.RemoveAll(typeof(ReadDbContext));
        //services.RemoveAll(typeof(WriteDbContext));
        //services.RemoveAll(typeof(IReadDbContext));
        //services.RemoveAll(typeof(IWriteDbContext));
        //services.RemoveAll(typeof(DbContext));


        var efServices = services.Where(d =>
            d.ServiceType.FullName != null && (
            d.ServiceType.FullName.StartsWith("Microsoft.EntityFrameworkCore") ||
            d.ServiceType.FullName.StartsWith("Pomelo.EntityFrameworkCore") ||
            d.ServiceType == typeof(DbContextOptions<WriteDbContext>) ||
            d.ServiceType == typeof(DbContextOptions<ReadDbContext>) ||
            d.ServiceType == typeof(WriteDbContext) ||
            d.ServiceType == typeof(ReadDbContext) ||
            d.ServiceType == typeof(IWriteDbContext) ||
            d.ServiceType == typeof(IReadDbContext))

        ).ToList();

            foreach (var service in efServices)
            {
                services.Remove(service);
            }

            _writeConnection = new SqliteConnection($"Data Source=write_db_{_uniqueId};Mode=Memory;Cache=Shared");
            _writeConnection.Open();

            _readConnection = new SqliteConnection($"Data Source=read_db_{_uniqueId};Mode=Memory;Cache=Shared");
            _readConnection.Open();

            services.AddDbContext<WriteDbContext>(options => options.UseSqlite(_writeConnection));
            services.AddScoped<IWriteDbContext, WriteDbContext>();

            services.AddDbContext<ReadDbContext>(options => options.UseSqlite(_readConnection));
            services.AddScoped<IReadDbContext, ReadDbContext>();



            //var sp = services.BuildServiceProvider();
            //using var scope = sp.CreateScope();

            //var writeContext = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            //writeContext.Database.EnsureCreated();

            //var readContext = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            //readContext.Database.EnsureCreated();


           
        });
    }


    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_readConnection != null)
        {
            await _readConnection.CloseAsync();
            await _readConnection.DisposeAsync();
        }

        if (_writeConnection != null)
        {
            await _writeConnection.CloseAsync();
            await _writeConnection.DisposeAsync();
        }
    }
}
