using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.QuerySync;
using VibeTree.User.CreateUser;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.CreateTest.Unitario;

public class CreateUserHandlerUnitario
{
    private readonly IWriteDbContext _dbMock = Substitute.For<IWriteDbContext>();
    private readonly ITokenService _tokenMock = Substitute.For<ITokenService>();
    private readonly IQueryJobSynchronize<SyncData<Domain.Entity.User>> _jobMock = Substitute.For<IQueryJobSynchronize<SyncData<Domain.Entity.User>>>();
    private readonly IValidator<CreateUserCommand> _validatorMock = Substitute.For<IValidator<CreateUserCommand>>();


    public CreateUserHandler createUser()=> new CreateUserHandler(_dbMock, _tokenMock, _validatorMock,_jobMock);

    [Fact]
    public async Task Handle_DeveRetonarFalhaQuandoCommandInvalido()
    {
        var erros = new ValidationResult(new[]
        {
            new ValidationFailure("Name",  "Nome obrigatório"),
            new ValidationFailure("Email", "Email inválido"),
            new ValidationFailure("Password", "Password obrigatório")
        });

        _validatorMock
            .ValidateAsync(Arg.Any<CreateUserCommand>())
            .Returns(erros);
        
        var createUserCommand = new CreateUserCommand("", "", "");
        var result = await createUser().HandleAsync(createUserCommand);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(erros.Errors.Count());

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _tokenMock.DidNotReceiveWithAnyArgs().CriarToken(default!);
        await _jobMock.DidNotReceiveWithAnyArgs().AddJob(default!);



    }
}
