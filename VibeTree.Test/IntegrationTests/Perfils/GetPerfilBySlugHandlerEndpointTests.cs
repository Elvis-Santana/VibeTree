using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlX.XDevAPI;
using System.Net.Http.Json;
using VibeTree.Features.Perfil;
using VibeTree.Features.Perfil.Get.GetBySlug;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using VibeTree.Shared.Entity;
using VibeTree.Test.Setup;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace VibeTree.Test.IntegrationTests.Perfils;

[Collection(IntegrationCollection.Name)]
public class GetPerfilBySlugHandlerEndpointTests(CustomWebApplicationFactory factory, ITestOutputHelper _output) : IAsyncLifetime
{
    private readonly HttpClient _httpClient = factory.CreateClient();
    private readonly ITestOutputHelper _testOutputHelper = _output;


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
    public async Task Shound_GetPerfilBySlug_Where_PerfilExists()
    {
        var (user, perfil) = Utils.GetUserAndPerfil();

         await using ( var scope = factory.Services.CreateAsyncScope()){
            var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await readDb.Users.AddAsync(user);
            await readDb.perfils.AddAsync(perfil);
            await readDb.SaveChangesAsync();

            await writeDb.Users.AddAsync(user);
            await writeDb.perfils.AddAsync(perfil);
            await writeDb.SaveChangesAsync();

        }
        var slugDoTeste = perfil.Slug;
        _testOutputHelper.WriteLine($"Slug do teste: {slugDoTeste}");

        var response = await this._httpClient.GetFromJsonAsync<Result<PerfilResponse>>($"/perfil/@{slugDoTeste}");


        response.IsSuccess.Should().BeTrue();
        response.Value.Id.Should().Be(perfil.Id);
        response.Value.IdUser.Should().Be(perfil.IdUser);
    }

    [Fact]
    public async Task Shound_NotFound_Where_PerfilNotExists()
    {
        string slug = new Faker("pt_BR").Internet.UserName();
        var response = await  this._httpClient.GetAsync($"/perfil/@{slug}");

        Result<PerfilResponse>? result = await response.Content.ReadFromJsonAsync<Result<PerfilResponse>>();

        result!.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);

    }
}

  
