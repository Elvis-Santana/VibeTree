using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Application.Interfaces;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.User.CreateUser;

namespace VibeTree.Test;

public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remover todas as entradas relacionadas ao EF registradas pela aplicação real
            services.RemoveAll(typeof(DbContextOptions<ReadDbContext>));
            services.RemoveAll(typeof(DbContextOptions<WriteDbContext>));
            services.RemoveAll(typeof(ReadDbContext));
            services.RemoveAll(typeof(WriteDbContext));
            services.RemoveAll(typeof(IReadDbContext));
            services.RemoveAll(typeof(IWriteDbContext));

            //services.RemoveAll(typeof(IHostedService));

            // Criar um provider EF isolado para o InMemory (evita conflito com Pomelo/MySql)
            ServiceProvider efInMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Registrar DbContexts com InMemory apenas para os testes e usar provider interno isolado
            services.AddDbContext<ReadDbContext>(options =>
                options.UseInMemoryDatabase("read", root)
                       .UseInternalServiceProvider(efInMemoryServiceProvider));

            services.AddDbContext<WriteDbContext>(options =>
                options.UseInMemoryDatabase("write", root)
                       .UseInternalServiceProvider(efInMemoryServiceProvider));

            // Garantir mapeamento das interfaces usadas pela aplicação
            services.AddScoped<IReadDbContext, ReadDbContext>();
            services.AddScoped<IWriteDbContext, WriteDbContext>();
        });

    }
}
