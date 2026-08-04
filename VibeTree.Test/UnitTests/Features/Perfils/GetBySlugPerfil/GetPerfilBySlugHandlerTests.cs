using FluentAssertions;
using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Features.Perfil.Get.GetBySlug;
using VibeTree.Shared.DbAppContext;
using VibeTree.Shared.Entity;

namespace VibeTree.Test.UnitTests.Features.Perfils.GetBySlugPerfil;

public class GetPerfilBySlugHandlerTests
{

    [Fact]
    public async Task Shoud_Retornar_Error_Nao_Encontrado()
    {
        await using var readDbContext = await new DbContextBuildConfig().CreateReadContext() ;

        var handler = new GetPerfilBySlugHandler(readDbContext);
        var result = await handler.Handle(new GetPerfilBySlugQuery("slug-nao-existe"));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors!.First().Message.Should().Be("perfil não encontrado");
    }
    [Fact]
    public async Task Shoud_Retornar_PerfilResponse_Quando_Encontrado()
    {
        var (user, perfil) = Utils.GetUserAndPerfil();

        await using (var writeDbContext = await new DbContextBuildConfig().CreateWriteContext())
        {
            await writeDbContext.Users.AddAsync(user);
            await writeDbContext.perfils.AddAsync(perfil);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = await new DbContextBuildConfig().CreateReadContext();
            await readDbContext.Users.AddAsync(user);
            await readDbContext.perfils.AddAsync(perfil);
            await readDbContext.SaveChangesAsync();

        var handler = new GetPerfilBySlugHandler(readDbContext);

        var result = await handler.Handle(new GetPerfilBySlugQuery(perfil.Slug));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(perfil.Id);
        result.Value.Descricao.Should().Be(perfil.Descricao);
        result.Value.ImagemUrl.Should().Be(perfil.ImagemUrl);
        result.Value.Slug.Should().Be(perfil.Slug);
        result.Value.IdUser.Should().Be(perfil.IdUser);
    }
}
