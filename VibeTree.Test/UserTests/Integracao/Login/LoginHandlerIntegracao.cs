using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlX.XDevAPI;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Application.User.Login;
using VibeTree.Domain.Entity;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Test.UserTests.Integracao.Login;

[Collection(IntegrationCollection.Name)]
public class LoginHandlerIntegracao(CustomWebApplicationFactory factory) : IAsyncLifetime
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


 
    [Fact]
    public async Task LoginHandler_Should_Return_Userlogin()
    {

        Guid id = Guid.NewGuid();
        string pwd = string.Empty;

        Domain.Entity.User user = new Faker<Domain.Entity.User>("pt_BR")
        .CustomInstantiator(f =>
        {
            pwd = f.Internet.Password();
            return new(
                 id,
                 DateTime.UtcNow,
                 DateTime.UtcNow,
                 f.Internet.UserName(),
                 BCrypt.Net.BCrypt.HashPassword(pwd),
                 f.Internet.Email()
            );
        }
        ).Generate();

        await AddUserToContextsAsync(user);

        LoginQuery loginQuery = new LoginQuery(pwd, user.Email);

        var response = await _client.PostAsJsonAsync<LoginQuery>("/auth/login", loginQuery);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();
        result.Value.Email.Should().Be(user.Email);
        result.Value.Name.Should().Be(user.Name);
        result.Value.Id.Should().Be(user.Id);

    }


    [Theory]
    [InlineData("", "")]
    [InlineData("13", "@email@.com")]
    [InlineData("sad123123", "@teste@gmail.com")]
    [InlineData("as", "teste@gmail.com")]
    public async Task LoginHandler_Should_Return_Error_Password_Incorrect(string pwd,string email)
    {
        Guid id = Guid.NewGuid();

        Domain.Entity.User user = new Faker<Domain.Entity.User>("pt_BR")
        .CustomInstantiator(f =>
        {
            return new(
                 id,
                 DateTime.UtcNow,
                 DateTime.UtcNow,
                 f.Internet.UserName(),
                 BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()),
                 f.Internet.Email()
            );
        }
        ).Generate();


        await AddUserToContextsAsync(user);

        LoginQuery loginQuery = new LoginQuery(pwd, email);

        var response = await _client.PostAsJsonAsync<LoginQuery>("/auth/login", loginQuery);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();
        result.IsSuccess.Should().BeFalse();

    }

    private async Task AddUserToContextsAsync(Domain.Entity.User user)
    {

        Perfil perfil = new(
              Guid.NewGuid(),
              DateTime.UtcNow,
              DateTime.UtcNow,
              string.Empty,
              string.Empty,
              string.Empty,
              user.Name,
              user.Id
          );

        using var scope = _factory.Services.CreateScope();

       
        using var readbSeed = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        await readbSeed.Users.AddAsync(user);
        await readbSeed.SaveChangesAsync();

        await readbSeed.perfils.AddAsync(perfil);
        await readbSeed.SaveChangesAsync();

    }
}