using Bogus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable;
using MockQueryable.NSubstitute;
using NSubstitute;
using System;
using System.Linq.Expressions;
using VibeTree.Application.Auth;
using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Application.User;
using VibeTree.Application.User.Create.Commands;
using VibeTree.Entity;
using VibeTree.Test.EntityTest;
using Wolverine;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.Unitario.Create;

public class CreateUserAndProfileHandlerUnitario
{
    private readonly IWriteDbContext _dbMock = Substitute.For<IWriteDbContext>();

    private readonly ITokenService _tokenMock =  Substitute.For<ITokenService>();

    private readonly IMessageContext _messageContextMock = Substitute.For<IMessageContext>();

    private readonly IValidator<CreateUserAndProfileCommand> _validatorMock =Substitute.For<IValidator<CreateUserAndProfileCommand>>();



    [Fact]
    public async Task Handle_DeveRetonarFalhaQuandoCommandInvalido()
    {
        var erros = new ValidationResult(new[]
        {
            new ValidationFailure("Name",  "Nome obrigatório"),
            new ValidationFailure("Email", "Email inválido"),
            new ValidationFailure("Password", "Password obrigatório"),
            new ValidationFailure("Slug", "Slug obrigatório")
        });

        _validatorMock
            .ValidateAsync(Arg.Any<CreateUserAndProfileCommand>())
            .Returns(erros);

        var createUserCommand = new CreateUserAndProfileCommand("", "", "", "");
        var (result,_) = await  CreateUserAndProfileHandler.Handle(createUserCommand, _dbMock, _tokenMock, _validatorMock);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(erros.Errors.Count());

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _tokenMock.DidNotReceiveWithAnyArgs().CriarToken(default!, default!);
    }

    [Fact]
    public async Task Handle_DeveRetonarFalhaExisteUser()
    {

        var faker = new Faker("pt_BR");
        var createUserCommand = new CreateUserAndProfileCommand(
            faker.Internet.UserName(),
            faker.Internet.Password(),
            faker.Internet.Email(),
            faker.Internet.UserName()

        );

        _validatorMock
            .ValidateAsync(Arg.Any<CreateUserAndProfileCommand>())
            .Returns(new ValidationResult());

        var usuarioExistente = new Domain.Entity.User(
            Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow,
            faker.Internet.UserName(),
            faker.Internet.Password(),
            createUserCommand.Email);

        var listaUsuarios = new List<User> { usuarioExistente };
        var usersSetMock = listaUsuarios.BuildMockDbSet();

        _dbMock.Users.Returns(usersSetMock);

        var (result,_) = await  CreateUserAndProfileHandler.Handle(createUserCommand, _dbMock, _tokenMock, _validatorMock); ;

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().Be(Error.ExisteUser);

    }

    [Fact]
    public async Task Handle_DeveCriarUsuarioComSucesso()
    {

        await using var name = new DbContextBuildConfig();
        var context = await name.CriarContextoWritePreparadoAsync();

        // Arrange
        var createUserCommand = new Faker<CreateUserAndProfileCommand>("pt_BR")
            .CustomInstantiator(f => new CreateUserAndProfileCommand(
            f.Person.FullName,
            f.Internet.Password(),
            f.Internet.Email(),
            f.Internet.UserName()
        )).Generate();


        string expectedToken = "token";

        _validatorMock
           .ValidateAsync(Arg.Any<CreateUserAndProfileCommand>())
           .Returns(new ValidationResult());

        var listaUsuarios = new List<User>();
        var usersSetMock = listaUsuarios.BuildMockDbSet();
        _dbMock.Users.Returns(usersSetMock);

        var listaPerfils = new List<Perfil>();
        var perfilsSetMock = listaPerfils.BuildMockDbSet();
        _dbMock.perfils.Returns(perfilsSetMock);



        _tokenMock.CriarToken(Arg.Any<User>(), Arg.Any<Perfil>())
            .Returns(Task.FromResult(new Token(expectedToken)));



        // Act
        var (result,_) = await   CreateUserAndProfileHandler.Handle(createUserCommand, context, _tokenMock, _validatorMock); 

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(createUserCommand.Name);
        result.Value.Email.Should().Be(createUserCommand.Email);
        result.Value.Token.Should().Be(expectedToken);
        result.Value.Id.Should().NotBe(Guid.Empty);

   
        await _tokenMock
            .Received()
            .CriarToken(Arg.Any<User>(), Arg.Any<Perfil>());
    }


}
