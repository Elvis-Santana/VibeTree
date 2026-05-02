using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;

namespace VibeTree.Test.ServiceTest.TokenServiceTest;

public class TokenServiceTest
{
    private readonly Faker _userFaker = new("pt_BR");

    private IConfiguration BuildConfig(string secret = "chave_de_teste_muito_longa_e_secreta_12345")
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:secret_key", secret}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void Constructor_WithValidConfiguration_DoesNotThrow()
    {
        var config = BuildConfig();

        Action act = () => new TokenService(config);

        act.Should().NotThrow();

    }

    [Fact]
    public void Constructor_WithInvalidConfiguration_Throw()
    {
        IConfiguration config =null;

        Action act = () => new TokenService(config);

        act.Should()
            .Throw()
            .WithoutMessage("A chave secreta não foi encontrada no appsettings.json");

    }

    [Fact]
    public async Task CriarToken_Should_RetornarToken()
    {

        var config = BuildConfig();
        var service = new TokenService(config);

        var user = new Domain.Entity.User(
             Guid.NewGuid(),
             DateTime.UtcNow,
             DateTime.UtcNow,
             _userFaker.Person.FullName,
             _userFaker.Internet.Password(),
             _userFaker.Internet.Email()
        );



        Token tokem = await service.CriarToken(user);


        tokem.token.Should().NotBeNullOrWhiteSpace();

    }
}
