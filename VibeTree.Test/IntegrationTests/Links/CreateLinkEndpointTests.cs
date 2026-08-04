using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Features.Link.CreateLink;
using VibeTree.Features.Link.CreateLink.Commands;
using VibeTree.Features.User;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using VibeTree.Shared.Entity;
using VibeTree.Test.Setup;
using Xunit.Abstractions;

namespace VibeTree.Test.IntegrationTests.Links;

[Collection(IntegrationCollection.Name)]

public class CreateLinkEndpointTests(CustomWebApplicationFactory factory, ITestOutputHelper _output) : IAsyncLifetime
{
    private readonly HttpClient _httpClient = factory.CreateClient();
    private IHost Host => factory.WolverineHost!;


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
    public async Task Should_ReturnOk_When_CreateLinkCommandIsValid()
    {
       var( user, perfil) =  Utils.GetUserAndPerfil();

        using (var scope = factory.Services.CreateScope())
        {
            using WriteDbContext write = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await StupEntity(user, perfil, write);

            using ReadDbContext read = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await StupEntity(user, perfil, read);
        }

        CreateLinkCommand command = Utils.GenerateCreateLinkCommandValid(perfil.Id.ToString());

        HttpResponseMessage response = await Utils.Cast(
            this.Host,
            Task.Run(async () => await _httpClient.PostAsJsonAsync("/link", command))
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        Result<LinkResponse> ?result = await response.Content.ReadFromJsonAsync<Result<LinkResponse>>();

        result!.IsSuccess.Should().BeTrue();

        result.Value.Descricao.Should().Be(command.Descricao);
        result.Value.LinkUrl.Should().Be(command.LinkUrl);
        result.Value.Order.Should().Be(command.Order);
        result.Value.Id.Should().NotBeEmpty();
        result.Value.IdPerfil.Should().Be(command.IdPerfil);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_CreateLinkCommandInvalid()
    {
       var( user, perfil) =  Utils.GetUserAndPerfil();

        using (var scope = factory.Services.CreateScope())
        {
            using WriteDbContext write = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await StupEntity(user, perfil, write);

            using ReadDbContext read = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await StupEntity(user, perfil, read);
        }

        CreateLinkCommand command = new (string.Empty,new string('a',256),string.Empty,default,default);

        HttpResponseMessage response = await Utils.Cast(
            this.Host,
            Task.Run(async () => await _httpClient.PostAsJsonAsync("/link", command))
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Result<LinkResponse> ?result = await response.Content.ReadFromJsonAsync<Result<LinkResponse>>();

        result!.IsSuccess.Should().BeFalse();

        result.Value.Should().BeNull();
        result.Errors.Should().HaveCount(3);

       
    }


    private static async Task StupEntity(User user, Perfil perfil, AbstractDbContext context)
    {
        await context.Users.AddAsync(user);
        await context.perfils.AddAsync(perfil);
        await context.SaveChangesAsync();
    }
}
