using FluentAssertions;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User.Create.Commands;
using VibeTree.Application.User.Delete.Commands;
using VibeTree.Application.User.Update.Commands;
using VibeTree.Infrastructure.AppDbContext;
using Wolverine;

namespace VibeTree.Test;


public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>, IAsyncLifetime
{
    private SqliteConnection? _readConnection;
    private SqliteConnection? _writeConnection;
    public IHost? WolverineHost { get; private set; }

    private readonly string _uniqueId = Guid.NewGuid().ToString();
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

        await writeDb.Database.EnsureCreatedAsync();
        await readDb.Database.EnsureCreatedAsync();
    }


    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        WolverineHost = host;
        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
  
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


            services.Configure<WolverineOptions>(opts =>
            {
                opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
                opts.Discovery.IncludeAssembly(typeof(CreateUserAndProfileCommand).Assembly);
                opts.Discovery.IncludeAssembly(typeof(DeleteUserCommand).Assembly);
                opts.Discovery.IncludeAssembly(typeof(UpdateUserCommand).Assembly);

            });
  

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
