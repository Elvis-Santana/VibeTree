using Bogus;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Entity;


namespace VibeTree.Test.EntityTest;

public class UserTest
{
    private readonly Faker _userFaker = new ("pt_BR");

    [Fact]
    public void User_Should_Have_Valid()
    {
        //arrange
        string name = _userFaker.Person.FullName;
        string passwordHash = _userFaker.Internet.Password();
        string email = _userFaker.Internet.Email();
        Guid id = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow;
        DateTime updatedAt = DateTime.UtcNow;



        //act
        User user = new (id, createdAt, updatedAt, name, passwordHash, email);


        //assert

        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.PasswordHash.Should().Be(passwordHash);
        user.Email.Should().Be(email);
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().Be(updatedAt);
    }
}
