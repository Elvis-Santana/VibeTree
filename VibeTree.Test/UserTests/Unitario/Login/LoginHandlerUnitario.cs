using Bogus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MockQueryable.NSubstitute;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User;
using VibeTree.Application.User.Login;
using VibeTree.Domain.Entity;
using VibeTree.User.CreateUser;
using BCryptNet = BCrypt.Net.BCrypt;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.Unitario.Login;

public class LoginHandlerUnitario
{
    private readonly IReadDbContext _readDbContextMock =
        Substitute.For<IReadDbContext>();

    private readonly ITokenService _tokenServiceMock =
        Substitute.For<ITokenService>();

    private readonly IValidator<LoginQuery> _validatorMock =
        Substitute.For<IValidator<LoginQuery>>();

    [Fact]
    public async Task Should_Retonar_Usuario_When_Dados_Validos()
    {

        const string token = "token------";
        const string pwd = "password";
        string pwdhash = BCryptNet.HashPassword(pwd);


        var user = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(u => new(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                u.Internet.UserName(),
                pwdhash,
                u.Internet.Email()
                )
            ).Generate();

        Perfil perfil = new(
               Guid.NewGuid(),
               DateTime.UtcNow,
               DateTime.UtcNow,
               string.Empty,
               string.Empty,
               string.Empty,
               user.Name,
               user.Id
           );

        LoginQuery query = new (pwd, user.Email);

        var expectedUserLogin = new Userlogin(user!.Id, user.Name, user.Email, token);
        _validatorMock.ValidateAsync(Arg.Any<LoginQuery>())
            .Returns(new ValidationResult());

        var listaUsuarios = new List<Domain.Entity.User> { user };
        var listaPerfils = new List<Domain.Entity.Perfil> { perfil };

        var usersSetMock = listaUsuarios.BuildMockDbSet();
        var perfilsSetMock = listaPerfils.BuildMockDbSet();

        _readDbContextMock.Users.Returns(usersSetMock);
        _readDbContextMock.perfils.Returns(perfilsSetMock);


        _tokenServiceMock.CriarToken(Arg.Any<Domain.Entity.User>(), Arg.Any<Domain.Entity.Perfil>())
            .Returns(Task.FromResult(new Token(token)));

        var handler =
            new LoginHandler(_readDbContextMock, _tokenServiceMock, _validatorMock);


        var result = await handler.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(user.Name);
        result.Value.Id.Should().Be(user.Id);
        result.Value.Email.Should().Be(user.Email);

        await _tokenServiceMock.Received()
            .CriarToken(Arg.Any<Domain.Entity.User>(), Arg.Any<Domain.Entity.Perfil>());

    }
    [Fact]
    public async Task Should_Retonar_Error_When_Dados_Invalidos()
    {
        var erros = new ValidationResult(new[]
        {
            new ValidationFailure("Email", "Email inválido"),
            new ValidationFailure("Password", "Password obrigatório")
        });

        _validatorMock
            .ValidateAsync(Arg.Any<LoginQuery>())
            .Returns(erros);


        var handler =
            new LoginHandler(_readDbContextMock, _tokenServiceMock, _validatorMock);


        LoginQuery query = new(string.Empty,string.Empty);

        var result = await handler.HandleAsync(query);


        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(erros.Errors.Count());

        await _tokenServiceMock
            .DidNotReceiveWithAnyArgs()
            .CriarToken(default!, default!);

    }
}
