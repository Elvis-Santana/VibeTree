using Bogus;
using FluentAssertions;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Perfil;
using VibeTree.Application.Perfil.Get.GetBySlug;
using VibeTree.Application.Result;
using VibeTree.Domain.Entity;

namespace VibeTree.Test.PerfilTest.GetSlugTest;

public class GetPerfilBySlugHandlerTest
{

    [Fact]
    public async Task GetPerfilBySlugHandler_Should_QueryBySlug_Perfil()
    {
        await using var db = new DbContextBuildConfig();
          var context = await db.CriarContextoReadPreparadoAsync();

        Domain.Entity.User user = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f => new(Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Internet.UserName(),
                f.Internet.Password(),
                f.Internet.Email())
            ).Generate();

        var perfil = new Faker<Perfil>("pt_BR")
           .CustomInstantiator(f =>
               new(Guid.NewGuid(),
               DateTime.Now,
               DateTime.Now,
               f.Internet.Color(),
               f.Lorem.Text(),
               f.Image.LoremFlickrUrl(),
               f.Internet.Url(),
               user.Id)
          ).Generate();

        await context.Users.AddAsync(user);
        await context.perfils.AddAsync(perfil);
        await context.SaveChangesAsync();

        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new GetPerfilBySlugHandler(context));

        GetPerfilBySlugQuery perfilBySlug = new(perfil.Slug);
        Result<PerfilResponse> result = await mediator.SendAsync(perfilBySlug);


        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(perfil.Id);
        result.Value.IdUser.Should().Be(perfil.IdUser);
    }

    [Fact]
    public async Task GetPerfilBySlugHandler_Should_QueryBySlug_Perfil_Error()
    {
        await using var db = new DbContextBuildConfig();
        var context = await db.CriarContextoReadPreparadoAsync();

     
        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new GetPerfilBySlugHandler(context));

        GetPerfilBySlugQuery perfilBySlug = new(Guid.NewGuid().ToString());
        Result<PerfilResponse> result = await mediator.SendAsync(perfilBySlug);


        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        
    }
}
