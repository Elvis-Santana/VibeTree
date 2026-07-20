using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using System.Runtime.CompilerServices;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Infrastructure.Worker;

namespace VibeTree.Test.WorkerTest.Unitario;

public class WorkerSynchronizeDbUnitario
{
    private static async IAsyncEnumerable<SyncData<T>> ToAsyncEnumerable<T>(IEnumerable<T> users, SyncOperation operation,
        [EnumeratorCancellation] CancellationToken ct = default
    )
    {
        foreach (var u in users)
        {
            if (ct.IsCancellationRequested) yield break;
            yield return new SyncData<T>(u, operation);

            await Task.Yield();
        }
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    public async Task WorkerSynchronizeUserDb_Deve_sincronizar_banco_de_Dados(int count)
    {
        // Arrange
        var usuarios = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f => new Domain.Entity.User(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Name.FullName(),
                f.Internet.Password(),
                f.Internet.Email()
            )).Generate(count);

        var jobMock = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.User>>>();

        jobMock.ReadAllAsync(Arg.Any<CancellationToken>())
               .Returns(ToAsyncEnumerable(usuarios, SyncOperation.Create));

        var dbMock = Substitute.For<IReadDbContext>();

        var loggerMock = Substitute.For<Microsoft.Extensions.Logging.ILogger<WorkerSynchronizeUserDb>>();


        var services = new ServiceCollection();
        services.AddScoped(_=> dbMock);
        ServiceProvider provider = services.BuildServiceProvider();

        var worker = new WorkerSynchronizeUserDb(jobMock, provider,loggerMock);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        //// Act
        await worker.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(3));

        //// Assert
        await dbMock.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WorkerSynchronizeUserDb_Exepection()
    {
        // Arrange
        var usuarios = new Faker<Domain.Entity.User>("pt_BR")
       .CustomInstantiator(f => new Domain.Entity.User(
           Guid.NewGuid(),
           DateTime.Now,
           DateTime.Now,
           f.Name.FullName(),
           f.Internet.Password(),
           f.Internet.Email()
       )).Generate(1);

        var jobMock = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.User>>>();

        jobMock.ReadAllAsync(Arg.Any<CancellationToken>())
               .Returns(ToAsyncEnumerable(usuarios, SyncOperation.Create));

        var dbMock = Substitute.For<IReadDbContext>();
        dbMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Falha ao salvar"));

        var loggerMock = Substitute.For<ILogger<WorkerSynchronizeUserDb>>();


        var services = new ServiceCollection();
        services.AddScoped<IReadDbContext>(_ => dbMock);
        ServiceProvider provider = services.BuildServiceProvider();

        var worker = new WorkerSynchronizeUserDb(jobMock, provider, loggerMock);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        await worker.StartAsync(cts.Token);

        // Assert
        loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains("Error processing batch")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );

    }

    [Fact]
    public async Task WorkerSynchronizeUserDb_Not_Save()
    {
        // Arrange

        var jobMock = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.User>>>();

        jobMock.ReadAllAsync(Arg.Any<CancellationToken>())
               .Returns(ToAsyncEnumerable(new List<Domain.Entity.User>(), SyncOperation.Create));

        var dbMock = Substitute.For<IReadDbContext>();

        var loggerMock = Substitute.For<ILogger<WorkerSynchronizeUserDb>>();


        var services = new ServiceCollection();
        services.AddScoped<IReadDbContext>(_ => dbMock);
        ServiceProvider provider = services.BuildServiceProvider();

        var worker = new WorkerSynchronizeUserDb(jobMock, provider, loggerMock);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        //// Act
        await worker.StartAsync(cts.Token);

        //// Assert
        await dbMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await dbMock.Users.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Domain.Entity.User>>(), Arg.Any<CancellationToken>());

    }
    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    public async Task WorkerSynchronizePerfilDb_Deve_sincronizar_banco_de_Dados(int count)
    {
        // Arrange
        var perfils = new Faker<Domain.Entity.Perfil>("pt_BR")
            .CustomInstantiator(f => new Domain.Entity.Perfil(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Internet.Color(),
                f.Lorem.Sentence(5, 233),
                f.Image.PicsumUrl(),
                f.Internet.UserName(),
                Guid.NewGuid()
            )).Generate(count);

        var jobMock = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.Perfil>>>();
        jobMock.ReadAllAsync(Arg.Any<CancellationToken>())
               .Returns(ToAsyncEnumerable(perfils, SyncOperation.Create));

        var dbMock = Substitute.For<IReadDbContext>();
        var loggerMock = Substitute.For<ILogger<WorkerSynchronizePerfilDb>>();

        var services = new ServiceCollection();
        services.AddScoped(_ => dbMock);
        IServiceProvider provider = services.BuildServiceProvider();

        var worker = new WorkerSynchronizePerfilDb(jobMock, provider, loggerMock);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        await worker.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Assert
        await dbMock.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WorkerSynchronizePerfilDb_Exepection()
    {
        // Arrange
        var perfils = new Faker<Domain.Entity.Perfil>("pt_BR")
              .CustomInstantiator(f => new Domain.Entity.Perfil(
                  Guid.NewGuid(),
                  DateTime.Now,
                  DateTime.Now,
                  f.Internet.Color(),
                  f.Lorem.Sentence(5, 233),
                  f.Image.PicsumUrl(),
                  f.Internet.UserName(),
                  Guid.NewGuid()
              )).Generate(1);

        var jobMock = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.Perfil>>>();
        jobMock.ReadAllAsync(Arg.Any<CancellationToken>())
               .Returns(ToAsyncEnumerable(perfils, SyncOperation.Create));

        var dbMock = Substitute.For<IReadDbContext>();
        dbMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Falha ao salvar"));

        var loggerMock = Substitute.For<Microsoft.Extensions.Logging.ILogger<WorkerSynchronizePerfilDb>>();

        var services = new ServiceCollection();
        services.AddScoped<IReadDbContext>(_ => dbMock);
        IServiceProvider provider = services.BuildServiceProvider();

        var worker = new WorkerSynchronizePerfilDb(jobMock, provider, loggerMock);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        await worker.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Assert
        await dbMock.Received().SaveChangesAsync(Arg.Any<CancellationToken>());

        // Assert
        loggerMock.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains("Error processing batch")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );

    }
}
