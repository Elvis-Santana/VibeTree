using Bogus;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Domain.Entity;

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
        Guid idUser = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        int order = _faker.Random.Int(1, 100);
        bool ativo = true;
        DateTime createdAt = DateTime.UtcNow;
        DateTime updatedAt = DateTime.UtcNow;

        //act

        var link = new Link(id, createdAt, updatedAt, linkUrl, descricao, idUser, order, ativo);
        //assert

        link.Id.Should().Be(id);
        link.CreatedAt.Should().Be(createdAt);
        link.UpdatedAt.Should().Be(updatedAt);
        link.LinkUrl.Should().Be(linkUrl);
        link.Descricao.Should().Be(descricao);
        link.IdUser.Should().Be(idUser);
        link.Order.Should().Be(order);
        link.Ativo.Should().Be(ativo);
    }
}
