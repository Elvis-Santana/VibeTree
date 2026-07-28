using Bogus;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Shared.Entity;

namespace VibeTree.Test.EntityTest;

public class LinkClickTest
{
    private readonly Faker _faker = new ("pt_BR");

    [Fact]
    public void LinkClick_Should_Have_Valid()
    {
        // arrange
        Guid idLink = Guid.NewGuid();
        long clickCount = _faker.Random.Long();
        Guid id = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;
        DateTime updatedAt = DateTime.UtcNow;
        // act
        LinkClick linkClick = new(id, createdAt, updatedAt, idLink, clickCount);
        // assert
        linkClick.Id.Should().Be(id);
        linkClick.IdLink.Should().Be(idLink);
        linkClick.ClickCount.Should().Be(clickCount);
        linkClick.CreatedAt.Should().Be(createdAt);
        linkClick.UpdatedAt.Should().Be(updatedAt);
    }
}
