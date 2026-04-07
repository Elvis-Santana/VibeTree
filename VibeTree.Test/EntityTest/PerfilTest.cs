using Bogus;
using FluentAssertions;
using NSubstitute.ClearExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Domain.Entity;

namespace VibeTree.Test.EntityTest;

public class PerfilTest
{
    private readonly Faker _perfilFaker = new ("pt_BR");


    [Fact]
    public void Perfil_Should_Have_Valid()
    {
        // arrange

         string cor = _perfilFaker.Random.String2(10,30);
         string descricao = _perfilFaker.Random.String2(10, 255);
         string imagemUrl = _perfilFaker.Internet.Url();
         Guid idUser = Guid.NewGuid();
         Guid id = Guid.NewGuid();
         DateTime createdAt = DateTime.UtcNow;
         DateTime updatedAt = DateTime.UtcNow;
        string slug = _perfilFaker.Random.String2(10, 30);

        //act

        Perfil perfil = new(id, createdAt,updatedAt, cor, descricao, imagemUrl, slug, idUser);

        //assert

        perfil.Id.Should().Be(id);
        perfil.Cor.Should().Be(cor);
        perfil.Descricao.Should().Be(descricao);
        perfil.ImagemUrl.Should().Be(imagemUrl);
        perfil.IdUser.Should().Be(idUser);
        perfil.CreatedAt.Should().Be(createdAt);
        perfil.UpdatedAt.Should().Be(updatedAt);

    }
}
