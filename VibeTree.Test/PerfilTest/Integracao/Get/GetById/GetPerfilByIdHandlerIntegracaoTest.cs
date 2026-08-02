using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Json;
using VibeTree.Features.Perfil;
using VibeTree.Features.Perfil.Get.GetById;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using VibeTree.Shared.Entity;
using Xunit.Abstractions;

namespace VibeTree.Test.PerfilTest.Integracao.Get.GetById;

[Collection(IntegrationCollection.Name)]

public class GetPerfilByIdHandlerIntegracaoTest(CustomWebApplicationFactory factory, ITestOutputHelper _output) : IAsyncLifetime
{
    private readonly HttpClient _httpClient = factory.CreateClient();


    public Task DisposeAsync() => Task.CompletedTask;

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
    public async Task Should_GetPerfilById_When_PerfilExists()
    {

        var (user, perfil) = Utils.GetUserAndPerfil();



        await using ( var scope = factory.Services.CreateAsyncScope())
        {
            var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

            await writeDb.Users.AddAsync(user);
            await writeDb.perfils.AddAsync(perfil);
            await writeDb.SaveChangesAsync();


            await readDb.Users.AddAsync(user);
            await readDb.perfils.AddAsync(perfil);
            await readDb.SaveChangesAsync();

        }

        var result = await _httpClient.GetFromJsonAsync<Result<PerfilResponse>>($"/perfil/{perfil.Id}");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(perfil.Id);
        result.Value.IdUser.Should().Be(perfil.IdUser);
    }

    [Fact]
    public async Task Should_NotFound_where_PerfilNotExist()
    {
        string id = Guid.NewGuid().ToString();
        var response = await this._httpClient.GetAsync($"/perfil/{id}");

        var result = await response.Content.ReadFromJsonAsync<Result<PerfilResponse>>();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
      
    }

  
}
