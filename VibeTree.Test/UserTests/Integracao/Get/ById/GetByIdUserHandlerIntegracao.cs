using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlX.XDevAPI;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VibeTree.Features.User;
using VibeTree.Shared.Auth;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using VibeTree.Shared.Entity;
using Xunit.Abstractions;

namespace VibeTree.Test.UserTests.Integracao.Get.ById;

[Collection(IntegrationCollection.Name)]
public class GetByIdUserHandlerIntegracao(CustomWebApplicationFactory factory, ITestOutputHelper _output) : IAsyncLifetime
{
    private readonly HttpClient _httpClient = factory.CreateClient();


    public Task DisposeAsync()=> Task.CompletedTask;

    public async Task InitializeAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();

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
    public async Task Deve_Criar_Retornar_Usuario_Case_Id_Valido_Ou_Se_Usuaior_Existe()
    {
        Guid id = Guid.NewGuid();
        (User user, Perfil perfil) userPerfil = StartEntityUserPerfil(id);

        await using var scope = factory.Services.CreateAsyncScope();

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        await AddUserToContextsAsync(userPerfil.user, userPerfil.perfil);

        Token token = await tokenService.CriarToken(userPerfil.user, userPerfil.perfil);

        var request = new HttpRequestMessage(HttpMethod.Get, "/user/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

        var response = await _httpClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Result<UserResponse>? result = await response.Content.ReadFromJsonAsync<Result<UserResponse>>();

        result!.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(userPerfil.user.Id);   
        result.Value.Name.Should().Be(userPerfil.user.Name);   
        result.Value.Email.Should().Be(userPerfil.user.Email);   

    }

    [Fact]

    public async Task Deve_Criar_Retornar_Erro_De_Id_Invalid_E_Usuaior_Nao_Existe()
    {
        Guid id = Guid.Empty;
        (User user, Perfil perfil) userPerfil = StartEntityUserPerfil(id);

        await using var scope = factory.Services.CreateAsyncScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        Token token = await tokenService.CriarToken(userPerfil.user, userPerfil.perfil);


        var request = new HttpRequestMessage(HttpMethod.Get, "/user/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.token);

        var response = await _httpClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        Result<UserResponse>? result = await response.Content.ReadFromJsonAsync<Result<UserResponse>>();
        result!.IsSuccess.Should().BeFalse();
        result.Errors!.First().Should().Be(Error.UserNotFound);

      
    }
    static (User user,  Perfil perfil) StartEntityUserPerfil(Guid id )
    {
        string pwd = string.Empty;
        User user  = new Faker<User>("pt_BR")
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
            }).Generate();

        return (user, perfil : new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            string.Empty,
            string.Empty,
            string.Empty,
            user.Name,
            user.Id
        ));
    }

    private async Task AddUserToContextsAsync(User user, Perfil perfil)
    {
        using var scope = factory.Services.CreateScope();

        using var readbSeed = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        await readbSeed.Users.AddAsync(user);
        await readbSeed.perfils.AddAsync(perfil);
        await readbSeed.SaveChangesAsync();

        using var writebSeed = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        await writebSeed.Users.AddAsync(user);
        await writebSeed.perfils.AddAsync(perfil);
        await writebSeed.SaveChangesAsync();
    }

}
