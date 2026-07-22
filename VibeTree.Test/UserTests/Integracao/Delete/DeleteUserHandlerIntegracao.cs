using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Application.Common;
using VibeTree.Application.User.Delete.Commands;
using VibeTree.Application.User.Login;
using VibeTree.Domain.Entity;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Test.UserTests.Integracao.Delete;

[Collection(IntegrationCollection.Name)]

public class DeleteUserHandlerIntegracao(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        await ClearDataAsync(writeDb);
        await ClearDataAsync(readDb);
    }

    private static async Task ClearDataAsync(DbContext db)
    {

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("a3b6c2d4-1234-5678-abcd-ef1234567890")]
    public async Task DeleteUserHandler_Should_Not_Found_User(string? urlParam)
    {
        Guid id = Guid.NewGuid();
        User user = new Faker<User>("pt_BR").CustomInstantiator(f => new(
            id,
            DateTime.UtcNow,
            DateTime.UtcNow,
            f.Internet.UserName(),
            BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()),
            f.Internet.Email()
            )
        ).Generate();

        Perfil perfil = new Faker<Perfil>("pt_BR").CustomInstantiator(f => new(
              Guid.NewGuid(),
              DateTime.UtcNow,
              DateTime.UtcNow,
              string.Empty,
              string.Empty,
              string.Empty,
              f.Internet.UserName(),
              user.Id
              )
        ).Generate();


        using (var scope = factory.Services.CreateScope())
        {
            using WriteDbContext write = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await StupEntity(user, perfil, write);

            using ReadDbContext read = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await StupEntity(user, perfil, read);

        }

        var response = await _client.DeleteAsync($"/user/{urlParam}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<Result<bool>>();

        content.IsSuccess.Should().BeFalse();
        content.Value.Should().BeFalse();




    }

    [Fact]
    public async Task DeleteUserHandler_Should_Delete_User()
    {
 
        Guid id = Guid.NewGuid();
        User user = new Faker<User>("pt_BR").CustomInstantiator(f => new(
            id,
            DateTime.UtcNow,
            DateTime.UtcNow,
            f.Internet.UserName(),
            BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()),
            f.Internet.Email()
            )
        ).Generate();

        Perfil perfil = new Faker<Perfil>("pt_BR").CustomInstantiator(f => new(
              Guid.NewGuid(),
              DateTime.UtcNow,
              DateTime.UtcNow,
              string.Empty,
              string.Empty,
              string.Empty,
              f.Internet.UserName(),
              user.Id
              )
        ).Generate();


        using (var scope = factory.Services.CreateScope())
        {
            using WriteDbContext write = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await StupEntity(user, perfil, write);

            using ReadDbContext read = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await StupEntity(user, perfil, read);

        }

        var response = await _client.DeleteAsync($"/user/{id.ToString()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<Result<bool>>();

        content.IsSuccess.Should().BeTrue();
        content.Value.Should().BeTrue();

        using (var scope = factory.Services.CreateScope())
        {
            using WriteDbContext write = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await VerifyUserNotFoundInContextAsync(user, write);

            using ReadDbContext read = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

            await VerifyUserNotFoundInContextAsync(user, read);

        }

        static async Task VerifyUserNotFoundInContextAsync(User user, AbstractDbContext context)
        {
            User? userNotExisteWrite = await context.Users.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));

            userNotExisteWrite.Should().BeNull();
        }
    }
    

    private static async Task StupEntity(User user, Perfil perfil, AbstractDbContext context)
    {
        await context.Users.AddAsync(user);
        await context.perfils.AddAsync(perfil);
        await context.SaveChangesAsync();
    }

}
