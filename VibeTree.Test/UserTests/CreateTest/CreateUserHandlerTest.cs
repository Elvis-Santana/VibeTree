using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.User.CreateUser;

namespace VibeTree.Test.UserTests.CreateUserTest;

public class CreateUserHandlerTest
{

    private readonly AppDbContext _context;

    public CreateUserHandlerTest()
    {
        var option = new DbContextOptionsBuilder<AppDbContext>()
              .UseInMemoryDatabase(new Guid().ToString())
              .Options;

           _context = new (option);
    }

    [Fact]
    public async Task CreateUserHandler_Should_Create_User()
    {

        var faker = new Faker<CreateUserCommand>("pt_BR")
        .CustomInstantiator(f => new ( f.Person.FullName, f.Person.Email,  f.Internet.Password()));

        CreateUserCommand user = faker.Generate();
        IHandler<CreateUserCommand, UserResponse> commandHandler = new CreateUserHandler(_context);


        UserResponse result = await commandHandler.HandleAsync(user);

        result.Name.Should().Be(user.name);
        result.Email.Should().Be(user.email);
        result.Id.Should().NotBeEmpty();



    }
}
