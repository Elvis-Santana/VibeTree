using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
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

        _context = new(option);
    }

    [Fact]
    public async Task CreateUserHandler_Should_Create_User()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        var faker = new Faker<CreateUserCommand>("pt_BR")
        .CustomInstantiator(f =>
        new(f.Person.FullName, BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()), f.Person.Email));

        CreateUserCommand user = faker.Generate();
        IHandler<CreateUserCommand, Result<Userlogin>> commandHandler = new CreateUserHandler(_context, service, new CreateUserValidator());


        Result<Userlogin> result = await commandHandler.HandleAsync(user);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(user.Name);
        result.Value.Email.Should().Be(user.Email);
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();



    }

    [Fact]
    public async Task CreateUserHandler_Should_Return_Error_When_Invalid_Data()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        CreateUserCommand user = new(string.Empty, string.Empty, string.Empty);

        IHandler<CreateUserCommand, Result<Userlogin>> commandHandler = new CreateUserHandler(_context, service, new CreateUserValidator());

        Result<Userlogin> result = await commandHandler.HandleAsync(user);
        result.Should().NotBeNull();

        result.IsSuccess.Should().BeFalse();
        result.Error.Contains(Error.EmailAddress).Should().BeTrue();
        result.Error.Contains(Error.EmailEmpty).Should().BeTrue();

        result.Error.Contains(Error.NameMinimumLength).Should().BeTrue();
        result.Error.Contains(Error.NameEmpty).Should().BeTrue();

        result.Error.Contains(Error.PasswordEmpty).Should().BeTrue();
        result.Error.Contains(Error.PasswordMinimumLength).Should().BeTrue();


    }
}