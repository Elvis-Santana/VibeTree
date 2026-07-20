using Bogus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Application.Result;
using VibeTree.Application.User.Delete;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.Unitario.Delete;

public class DeleteUserHandlerUnitario
{
    private readonly IWriteDbContext _dbMock = Substitute.For<IWriteDbContext>();
    private readonly IQueueSynchronizeDb<SyncData<Domain.Entity.User>> _jobMock = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.User>>>();
    private readonly IValidator<DeleteUserCommand> _validatorMock = Substitute.For<IValidator<DeleteUserCommand>>();

    public DeleteUserHandler DeleteHandler() => new DeleteUserHandler(_dbMock, _validatorMock, _jobMock  );

    [Fact]
    public async Task Handler_Deve_Retornar_Id_Vazio()
    {
        var error = new ValidationResult(new[]
        {
            new ValidationFailure("Id","id não pode ser vazio ")
        });

        _validatorMock.ValidateAsync(Arg.Any<DeleteUserCommand>())
            .Returns(error);

        DeleteUserCommand deleteUserCommand = new (string.Empty);

        var hanlder = await DeleteHandler().HandleAsync(deleteUserCommand);

        hanlder.IsSuccess.Should().BeFalse();
        hanlder.Errors.Should().HaveCount(1);

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _jobMock.DidNotReceiveWithAnyArgs().AddJobAsync(default!);

    }

    [Fact]
    public async Task Handler_Deve_Retornar_Erro_de_Id_Invalido()
    {


        _validatorMock.ValidateAsync(Arg.Any<DeleteUserCommand>())
            .Returns(new ValidationResult());

        DeleteUserCommand deleteUserCommand = new(Guid.NewGuid().ToString());
        _dbMock.Users.FindAsync(Arg.Any<Guid>())!.Returns(ValueTask.FromResult<Domain.Entity.User>(null));

        var hanlder = await DeleteHandler().HandleAsync(deleteUserCommand);

        hanlder.IsSuccess.Should().BeFalse();
        hanlder.Errors!.Should().Contain(Error.NotFound);

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _jobMock.DidNotReceiveWithAnyArgs().AddJobAsync(default!);

    }

    [Fact]
    public async Task Handler_Deve_Deletar_Usuario()
    {

        var user = new Faker<Domain.Entity.User>("pt_BR")
             .CustomInstantiator(u => new Domain.Entity.User(
                 Guid.NewGuid(),
                 DateTime.UtcNow,
                 DateTime.UtcNow,
                 u.Internet.UserName(),
                 BCrypt.Net.BCrypt.HashPassword(u.Internet.Password()),
                 u.Internet.Email())
             ).Generate(); 

        _validatorMock.ValidateAsync(Arg.Any<DeleteUserCommand>())
            .Returns(new ValidationResult());

        DeleteUserCommand deleteUserCommand = new(user.Id.ToString());
        _dbMock.Users.FindAsync(Arg.Any<Guid>())!.Returns(ValueTask.FromResult(user));

        _dbMock.Users.Remove(Arg.Any<Domain.Entity.User>());

        _dbMock.SaveChangesAsync().Returns(Task.FromResult(1));

        var hanlder = await DeleteHandler().HandleAsync(deleteUserCommand);

        hanlder.IsSuccess.Should().BeTrue();
        hanlder.Value.Should().BeTrue();

        await _dbMock.Received().SaveChangesAsync();
        await _jobMock.Received().AddJobAsync(Arg.Any<SyncData<Domain.Entity.User>>());

    }

}
