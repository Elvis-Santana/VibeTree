using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Features.Perfil.Get.GetById;
using VibeTree.Features.Perfil.Get.GetBySlug;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Test.PerfilTest.Unitario.Get.GetById;

public class GetPerfilByIdHandlerUnitarioTest(DbContextBuildConfig _dbFixture) : IClassFixture<DbContextBuildConfig>
{
  

    [Fact]
    public async Task Shoud_Retornar_Error_Nao_Encontrado()
    {
        await using var readDbContext = await _dbFixture.CreateReadContext();

        var handler = new GetPerfilByIdHandler(readDbContext);
        var result = await handler.Handle(new GetPerfilByIdQuery(Guid.NewGuid().ToString()));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors!.First().Message.Should().Be("perfil não encontrado");
    }
    [Fact]
    public async Task Shoud_Retornar_PerfilResponse_Quando_Encontrado()
    {
        var (user, perfil) = Utils.GetUserAndPerfil();
        await using (var writeDbContext = await _dbFixture.CreateWriteContext())
        {
            await writeDbContext.Users.AddAsync(user);
            await writeDbContext.perfils.AddAsync(perfil);
            await writeDbContext.SaveChangesAsync();
        }

        await using var readDbContext = await _dbFixture.CreateReadContext();
    

        var handler = new GetPerfilByIdHandler(readDbContext);

        var result = await handler.Handle(new GetPerfilByIdQuery(perfil.Id.ToString()));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(perfil.Id);
        result.Value.Descricao.Should().Be(perfil.Descricao);
        result.Value.ImagemUrl.Should().Be(perfil.ImagemUrl);
        result.Value.Slug.Should().Be(perfil.Slug);
        result.Value.IdUser.Should().Be(perfil.IdUser);
    }
}
