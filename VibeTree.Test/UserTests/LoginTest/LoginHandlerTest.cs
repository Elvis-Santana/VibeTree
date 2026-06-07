using Bogus;
using FluentAssertions;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Application.User.Login;

namespace VibeTree.Test.UserTests.LoginTest;

public class LoginHandlerTest
{

  

    [Fact]
    public async Task LoginHandler_Should_Return_Userlogin()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        await using var db = new DbContextBuildConfig();
         var context = await db.CriarContextoReadPreparadoAsync();

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

        Domain.Entity.User users = userFaker.Generate();
        await context.Users.AddAsync(users);
        await context.SaveChangesAsync();

        LoginQuery loginQuerie = context.Users.Select(u => new LoginQuery(password, u.Email)).First();

        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new LoginHandler(context, service, new LoginValidator()));

        Result<Userlogin> result = await mediator.SendAsync(loginQuerie);

        result.Value.Email.Should().Be(loginQuerie.email);
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();

    }

    [Fact]
    public async Task LoginHandler_Should_Return_Error_Password_Incorrect()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        await using var db = new DbContextBuildConfig();
        var context = await db.CriarContextoReadPreparadoAsync();
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
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var loginQuerie = new Faker<LoginQuery>("pt_BR")
            .CustomInstantiator(f => new LoginQuery(
                f.Internet.Password(),
                f.Internet.Email()
            )).Generate();


        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new LoginHandler(context, service, new LoginValidator()));
        Result<Userlogin> result = await mediator.SendAsync(loginQuerie);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

    }


    [Fact]
    public async Task LoginHandler_Should_Return_Error_EmailEmpty_PasswordEmpty()
    {
        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);
        await using var db = new DbContextBuildConfig();
          var context = await db.CriarContextoReadPreparadoAsync();

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

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var loginQuerie = new LoginQuery(string.Empty, string.Empty);


        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new LoginHandler(context, service, new LoginValidator()));

        Result<Userlogin> result = await mediator.SendAsync(loginQuerie);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

    }
}