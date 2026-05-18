using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Application.User.Login;
using VibeTree.Domain.Entity;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.User.CreateUser;

namespace VibeTree.Test.UserTests.LoginTest;

public class LoginHandlerTest
{
    private readonly AppDbContext _context;

    public LoginHandlerTest()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task LoginHandler_Should_Return_Userlogin()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        string password = new Faker("pt_BR").Internet.Password();



        var userFaker = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f => new Domain.Entity.User(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Name.FullName(),
                BCrypt.Net.BCrypt.HashPassword(password),
                f.Internet.Email()
            ));

        var users = userFaker.Generate();

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var loginQuerie = _context.Users.Select(u => new LoginQuery(password, u.Email)).First();


        IHandler<LoginQuery, Result<Userlogin>> queryHandler =
            new LoginHandler(_context, service, new LoginValidator());

        Result<Userlogin> result = await queryHandler.HandleAsync(loginQuerie);

        result.Value.Email.Should().Be(loginQuerie.email);
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();

    }

    [Fact]
    public async Task LoginHandler_Should_Return_Error_Password_Incorrect()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        string password = new Faker("pt_BR").Internet.Password();

        var userFaker = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f => new Domain.Entity.User(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Name.FullName(),
                BCrypt.Net.BCrypt.HashPassword(password),
                f.Internet.Email()
            ));

        var users = userFaker.Generate();

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var loginQuerie = new Faker<LoginQuery>("pt_BR")
            .CustomInstantiator(f => new LoginQuery(
                f.Internet.Password(),
                f.Internet.Email()
            )).Generate();

        IHandler<LoginQuery, Result<Userlogin>> queryHandler = new LoginHandler(_context, service, new LoginValidator());
        Result<Userlogin> result = await queryHandler.HandleAsync(loginQuerie);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

    }


    [Fact]
    public async Task LoginHandler_Should_Return_Error_EmailEmpty_PasswordEmpty()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        string password = new Faker("pt_BR").Internet.Password();

        var userFaker = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f => new Domain.Entity.User(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Name.FullName(),
                BCrypt.Net.BCrypt.HashPassword(password),
                f.Internet.Email()
            ));

        var users = userFaker.Generate();

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var loginQuerie = new LoginQuery(string.Empty, string.Empty);

        IHandler<LoginQuery, Result<Userlogin>> queryHandler = new LoginHandler(_context, service, new LoginValidator());
        Result<Userlogin> result = await queryHandler.HandleAsync(loginQuerie);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

    }
}