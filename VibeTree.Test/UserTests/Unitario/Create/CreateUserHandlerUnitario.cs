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
using System.Linq.Expressions;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Result;
using VibeTree.Application.Sync;
using VibeTree.Application.User;
using VibeTree.Test.EntityTest;
using VibeTree.User.CreateUser;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.Unitario.Create;

public class CreateUserHandlerUnitario
{
    private readonly IWriteDbContext _dbMock = Substitute.For<IWriteDbContext>();

    private readonly ITokenService _tokenMock =  Substitute.For<ITokenService>();

    private readonly IQueueSynchronizeDb<SyncData<Domain.Entity.User>> _jobMockUser = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.User>>>();
    private readonly IQueueSynchronizeDb<SyncData<Domain.Entity.Perfil>> _jobMockPerfil = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.Perfil>>>();

    private readonly IValidator<CreateUserAndProfileCommand> _validatorMock =Substitute.For<IValidator<CreateUserAndProfileCommand>>();


    public CreateUserAndProfileHandler createUser()=> new CreateUserAndProfileHandler(_dbMock, _tokenMock, _validatorMock, _jobMockUser, _jobMockPerfil);

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
        
        var createUserCommand = new CreateUserAndProfileCommand("", "", "","");
        var result = await createUser().HandleAsync(createUserCommand);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(erros.Errors.Count());

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _tokenMock.DidNotReceiveWithAnyArgs().CriarToken(default!, default!);
        await _jobMockUser.DidNotReceiveWithAnyArgs().AddJobAsync(default!);
        await _jobMockPerfil.DidNotReceiveWithAnyArgs().AddJobAsync(default!);
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

        var listaUsuarios = new List<Domain.Entity.User> { usuarioExistente };
        var usersSetMock = listaUsuarios.BuildMockDbSet();

        _dbMock.Users.Returns(usersSetMock);

        var result = await createUser().HandleAsync(createUserCommand);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().Be(Error.ExisteUser);

    }

    [Fact]
    public async Task Handle_DeveCriarUsuarioComSucesso()
    {

        await using  var name = new DbContextBuildConfig();
        var context = await name.CriarContextoWritePreparadoAsync();

        // Arrange
        var faker = new Faker("pt_BR");

        var createUserCommand = new CreateUserAndProfileCommand(
            faker.Person.FullName,
            faker.Internet.Password(),
            faker.Internet.Email(),
            faker.Internet.UserName() 
        );

        string expectedToken = "token";

        _validatorMock
           .ValidateAsync(Arg.Any<CreateUserAndProfileCommand>())
           .Returns(new ValidationResult());

        var listaUsuarios = new List<Domain.Entity.User>();
        var usersSetMock = listaUsuarios.BuildMockDbSet();
        _dbMock.Users.Returns(usersSetMock);

        var listaPerfils = new List<Domain.Entity.Perfil>();
        var perfilsSetMock = listaPerfils.BuildMockDbSet();
        _dbMock.perfils.Returns(perfilsSetMock);

       

        _tokenMock.CriarToken(Arg.Any<Domain.Entity.User>(), Arg.Any<Domain.Entity.Perfil>())
            .Returns(Task.FromResult(new Token(expectedToken)));

        _jobMockUser.AddJobAsync(Arg.Any<SyncData<Domain.Entity.User>>()).Returns(ValueTask.CompletedTask);
        _jobMockPerfil.AddJobAsync(Arg.Any<SyncData<Domain.Entity.Perfil>>()).Returns(ValueTask.CompletedTask);

        // Act
        var h = new CreateUserAndProfileHandler(context, _tokenMock, _validatorMock, _jobMockUser, _jobMockPerfil); ;
        var result = await h.HandleAsync(createUserCommand);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(createUserCommand.Name);
        result.Value.Email.Should().Be(createUserCommand.Email);
        result.Value.Token.Should().Be(expectedToken);
        result.Value.Id.Should().NotBe(Guid.Empty);


        await _jobMockUser
            .Received()
            .AddJobAsync(Arg.Any<SyncData<Domain.Entity.User>>());

        await _jobMockPerfil
          .Received()
          .AddJobAsync(Arg.Any<SyncData<Domain.Entity.Perfil>>());


        await _tokenMock
            .Received()
            .CriarToken(Arg.Any<Domain.Entity.User>(), Arg.Any<Domain.Entity.Perfil>());


    }


}
