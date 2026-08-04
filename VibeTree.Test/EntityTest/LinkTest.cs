using Bogus;
using FluentAssertions;
using VibeTree.Shared.Entity;

namespace VibeTree.Test.EntityTest;

public class LinkTest
{
    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public void Link_Should_Have_Valid()
    {
        // Arrange
        string linkUrl = _faker.Internet.Url();
        string descricao = _faker.Lorem.Sentence();
        Guid perfil = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        decimal order = _faker.Random.Decimal(1, 1000);
        bool ativo = true;
        DateTime createdAt = DateTime.UtcNow;
        DateTime updatedAt = DateTime.UtcNow;

        //act

        var link = new Link(id, createdAt, updatedAt, linkUrl, descricao, perfil, order, ativo);
        //assert

        link.Id.Should().Be(id);
        link.CreatedAt.Should().Be(createdAt);
        link.UpdatedAt.Should().Be(updatedAt);
        link.LinkUrl.Should().Be(linkUrl);
        link.Descricao.Should().Be(descricao);
        link.IdPerfil.Should().Be(perfil);
        link.Order.Should().Be(order);
        link.Ativo.Should().Be(ativo);
    }
}
