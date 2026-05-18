using Bogus;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute.ExceptionExtensions;
using System;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Perfil;
using VibeTree.Application.Perfil.Create;
using VibeTree.Application.Result;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Test.PerfilTest.CreateTest;

public class CreatePerfilHandlerTest
{

  

    [Fact]
    public async Task CreatePerfilHandler_Should_Create_Perfil()
    {
        await using var db = new DbContextBuildConfig();

        await using var context = await db.CriarContextoPreparadoAsync();
        var faker = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f =>  new (Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Internet.UserName(),
                f.Internet.Password(),
                f.Internet.Email())
            );

        Domain.Entity.User user =  faker.Generate();

        await context.Users.AddAsync( user );
        await context.SaveChangesAsync();

        IHandler<CreatePerfilCommand, Result<PerfilResponse>> commandHandler =
            new CreatePerfilHandler(context);

       var createPerfilCommand = new Faker<CreatePerfilCommand>("pt_BR")
            .CustomInstantiator(f =>
            new(f.Internet.Color(), f.Lorem.Text(), f.Image.LoremFlickrUrl(), f.Internet.Url(), user.Id)).Generate();

        var result = await commandHandler.HandleAsync(createPerfilCommand);


        result.IsSuccess.Should().BeTrue();
        result.Value.IdUser.Should().Be(user.Id);
        result.Value.Descricao.Should().Be(createPerfilCommand.Descricao);
        result.Value.ImagemUrl.Should().Be(createPerfilCommand.ImagemUrl);
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Slug.Should().Be(createPerfilCommand.Slug);
    }

    [Fact]
    public async Task CreatePerfilHandler_Should_Create_Perfil_Error()
    {
        
       await using var db  =  new DbContextBuildConfig();

       await using var context = await db.CriarContextoPreparadoAsync();

        var faker = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f => new(Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Internet.UserName(),
                f.Internet.Password(),
                f.Internet.Email())
            );

        Domain.Entity.User user = faker.Generate();

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        IHandler<CreatePerfilCommand, Result<PerfilResponse>> commandHandler =
            new CreatePerfilHandler(context);

        var createPerfilCommand = new Faker<CreatePerfilCommand>("pt_BR")
             .CustomInstantiator(f =>
             new(
                 f.Internet.Color(),
                 f.Lorem.Text(),
                 f.Image.LoremFlickrUrl(),
                 f.Internet.Url(),
                 Guid.NewGuid())).Generate();

        await commandHandler.Invoking(
            ch => ch.HandleAsync(createPerfilCommand)
            ).Should().ThrowAsync<DbUpdateException>();

    }
}
