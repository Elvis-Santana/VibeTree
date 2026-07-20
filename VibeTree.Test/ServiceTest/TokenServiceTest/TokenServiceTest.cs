using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Domain.Entity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace VibeTree.Test.ServiceTest.TokenServiceTest;

public class TokenServiceTest
{
    private readonly Faker _userFaker = new("pt_BR");



    [Fact]
    public void Constructor_WithValidConfiguration_DoesNotThrow()
    {
        var config = JwtBuildConfig.BuildConfig();

        Action act = () => new TokenService(config);

        act.Should().NotThrow();

    }

    [Fact]
    public void Constructor_WithInvalidConfiguration_Throw()
    {
        IConfiguration config = null;

        Action act = () => new TokenService(config);

        act.Should()
            .Throw()
            .WithoutMessage("A chave secreta não foi encontrada no appsettings.json");

    }

    [Fact]
    public async Task CriarToken_Should_RetornarToken()
    {

        var config = JwtBuildConfig.BuildConfig(); 
        var service = new TokenService(config);

        var user = CriarUsuarioValido();
        var perfil = CriarPerfilValidoPara(user);

        Token token = await service.CriarToken(user, perfil);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token.token);

        var usuarioId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var idPerfilClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "perfil_Id")?.Value;

        usuarioId.Should().Be(user.Id.ToString());
        idPerfilClaim.Should().Be(perfil.Id.ToString());

    }

    private Domain.Entity.User CriarUsuarioValido()
    {
        return new Domain.Entity.User(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            _userFaker.Person.FullName,
            _userFaker.Internet.Password(),
            _userFaker.Internet.Email()
        );
    }

    private Perfil CriarPerfilValidoPara(Domain.Entity.User user)
    {
        return new Perfil(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            string.Empty,
            string.Empty,
            string.Empty,
            user.Name,
            user.Id
        );
    }
}
