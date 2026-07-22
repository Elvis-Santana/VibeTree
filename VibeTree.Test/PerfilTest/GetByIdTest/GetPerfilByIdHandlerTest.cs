using Bogus;
using FluentAssertions;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Perfil;
using VibeTree.Application.Perfil.Get.GetById;
using VibeTree.Application.Common;
using VibeTree.Domain.Entity;

namespace VibeTree.Test.PerfilTest.GetByIdTest;

public class GetPerfilByIdHandlerTest
{

    [Fact]
    public async Task GetPerfilByIdHandler_Should_QueryById_Perfil()
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

        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new GetPerfilByIdHandler(context));

        GetPerfilByIdQuery perfilByIdQuery = new (perfil.Id.ToString());
        Result<PerfilResponse> result = await mediator.SendAsync(perfilByIdQuery);


        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(perfil.Id);
        result.Value.IdUser.Should().Be(perfil.IdUser);
    }

    [Fact]
    public async Task GetPerfilByIdHandler_Should_Not_Found_Perfil()
    {
        await using var db = new DbContextBuildConfig();
         var context = await db.CriarContextoReadPreparadoAsync();

        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new GetPerfilByIdHandler(context));

        GetPerfilByIdQuery perfilByIdQuery = new(Guid.NewGuid().ToString());
        Result<PerfilResponse> result = await mediator.SendAsync(perfilByIdQuery);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
      
    }

}
