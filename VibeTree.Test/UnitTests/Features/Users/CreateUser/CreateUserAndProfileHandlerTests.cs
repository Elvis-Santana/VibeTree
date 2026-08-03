using Bogus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MockQueryable.NSubstitute;
using NSubstitute;
using VibeTree.Features.User.CreateUser.Commands;
using VibeTree.Shared.Auth;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using VibeTree.Shared.Entity;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UnitTests.Features.Users.CreateUser;

public class CreateUserAndProfileHandlerTests(DbContextBuildConfig _dbFixture) : IClassFixture<DbContextBuildConfig>
{

    private readonly ITokenService _tokenMock =  Substitute.For<ITokenService>();


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
        using var writeDbContext = await _dbFixture.CreateWriteContext();
        var (result,_) = await  CreateUserAndProfileHandler.Handle(createUserCommand, writeDbContext, _tokenMock, _validatorMock);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(erros.Errors.Count());

        await _tokenMock.DidNotReceiveWithAnyArgs().CriarToken(default!, default!);
    }

    [Fact]
    public async Task Handle_DeveRetonarFalhaExisteUser()
    {

        var faker = new Faker("pt_BR");
        var createUserCommand = new Faker<CreateUserAndProfileCommand>("pt_BR")
            .CustomInstantiator(f => new(
            faker.Internet.UserName(),
            faker.Internet.Password(),
            faker.Internet.Email(),
            faker.Internet.UserName()
        )).Generate();

        _validatorMock
            .ValidateAsync(Arg.Any<CreateUserAndProfileCommand>())
            .Returns(new ValidationResult());

        var usuarioExistente = new User(
            Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow,
            faker.Internet.UserName(),
            faker.Internet.Password(),
            createUserCommand.Email);

        using var writeDbContext = await _dbFixture.CreateWriteContext();
        await writeDbContext.Users.AddAsync(usuarioExistente);
        await writeDbContext.SaveChangesAsync();

        var (result,_) = await  CreateUserAndProfileHandler.Handle(createUserCommand, writeDbContext, _tokenMock, _validatorMock); ;

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().Should().Be(Error.ExisteUser);

    }

    [Fact]
    public async Task Handle_DeveCriarUsuarioComSucesso()
    {
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

        using var writeDbContext = await _dbFixture.CreateWriteContext();
     
        _tokenMock.CriarToken(Arg.Any<User>(), Arg.Any<Perfil>())
            .Returns(Task.FromResult(new Token(expectedToken)));

        // Act
        var (result,_) = await   CreateUserAndProfileHandler.Handle(createUserCommand, writeDbContext, _tokenMock, _validatorMock); 

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
