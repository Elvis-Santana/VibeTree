using Bogus;
using FluentAssertions;
using VibeTree.Features.Link.CreateLink.Commands;

namespace VibeTree.Test.UnitTests.Features.Links.CreateLink;

public class CreateLinkHandlerTests 
{

  
    [Fact]
    public async Task Should_RetornarErro_When_InvalidLinkUrl()
    {
       
       await using var context = await new DbContextBuildConfig().CreateWriteContext();
        var handler = new CreateLinkHandler(context, new CreateLinkValidator());
        var command = Utils.GenerateCreateLinkCommandInvalids("LinkUrl");

        var (result, @event) = await handler.Handle(command);

        result.IsSuccess.Should().BeFalse();
        @event.Should().BeNull();
    }

    [Fact]
    public async Task Should_RetornarErro_When_InvalidDescricao()
    {
        await using var context = await new DbContextBuildConfig().CreateWriteContext();
        var handler = new CreateLinkHandler(context, new CreateLinkValidator());
        var command = Utils.GenerateCreateLinkCommandInvalids("Descricao");

        var (result, @event) = await handler.Handle(command);

        result.IsSuccess.Should().BeFalse();
        @event.Should().BeNull();

    }

    [Fact]
    public async Task Should_RetornarErro_When_InvalidIdPerfil()
    {

        await using var context = await new DbContextBuildConfig().CreateWriteContext();
        var handler = new CreateLinkHandler(context, new CreateLinkValidator());
        var command = Utils.GenerateCreateLinkCommandInvalids("IdPerfil");

        var (result, @event) = await handler.Handle(command);

        result.IsSuccess.Should().BeFalse();
        @event.Should().BeNull();

        
    }

    [Fact]
    public async Task Should_RetornarDtoLinkResponse_When_DataValid()
    {
        var (user, perfil) = Utils.GetUserAndPerfil();
        await using var db =  new DbContextBuildConfig();

        using var contextWrite = await db.CreateWriteContext();
        using var contextRead = await db.CreateReadContext();

        await contextWrite.Users.AddAsync(user);
        await contextWrite.perfils.AddAsync(perfil);
        await contextWrite.SaveChangesAsync();


        await contextRead.Users.AddAsync(user);
        await contextRead.perfils.AddAsync(perfil);
        await contextRead.SaveChangesAsync();

        var handler = new CreateLinkHandler(contextWrite, new CreateLinkValidator());

        var command = new Faker<CreateLinkCommand>("pt_BR")
          .CustomInstantiator(r =>
          new(r.Lorem.Letter(10), r.Lorem.Letter(255), perfil.Id.ToString(), 1, true)
         );

        var (result, @event) = await handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        @event.Should().NotBeNull();
    }


           
    


}
