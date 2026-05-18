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

  

    [Fact]
    public async Task CreateUserHandler_Should_Create_User()
    {

        await using var db = new DbContextBuildConfig();
        await using var context = await db.CriarContextoPreparadoAsync();

        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        var faker = new Faker<CreateUserCommand>("pt_BR")
        .CustomInstantiator(f =>
        new(f.Person.FullName, BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()), f.Person.Email));

        CreateUserCommand user = faker.Generate();
        IHandler<CreateUserCommand, Result<Userlogin>> commandHandler = new CreateUserHandler(context, service, new CreateUserValidator());


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
        await using var db = new DbContextBuildConfig();
        await using var context = await db.CriarContextoPreparadoAsync();

        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        CreateUserCommand user = new(string.Empty, string.Empty, string.Empty);

        IHandler<CreateUserCommand, Result<Userlogin>> commandHandler = new CreateUserHandler(context, service, new CreateUserValidator());

        Result<Userlogin> result = await commandHandler.HandleAsync(user);
        result.Should().NotBeNull();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Contains(Error.EmailAddress).Should().BeTrue();
        result.Errors.Contains(Error.EmailEmpty).Should().BeTrue();

        result.Errors.Contains(Error.NameMinimumLength).Should().BeTrue();
        result.Errors.Contains(Error.NameEmpty).Should().BeTrue();

        result.Errors.Contains(Error.PasswordEmpty).Should().BeTrue();
        result.Errors.Contains(Error.PasswordMinimumLength).Should().BeTrue();


    }
}