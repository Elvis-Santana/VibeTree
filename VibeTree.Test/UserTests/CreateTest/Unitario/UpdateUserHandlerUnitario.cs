using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Application.Result;
using VibeTree.Application.User.Update;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.CreateTest.Unitario;

public class UpdateUserHandlerUnitario
{
    private readonly IWriteDbContext _dbMock = Substitute.For<IWriteDbContext>();
    private readonly ITokenService _tokenMock = Substitute.For<ITokenService>();

    private readonly IQueueSynchronizeDb<SyncData<Domain.Entity.User>> _jobMock = Substitute.For<IQueueSynchronizeDb<SyncData<Domain.Entity.User>>>();
    private readonly IValidator<UpdateUserCommand> _validatorMock = Substitute.For<IValidator<UpdateUserCommand>>();

    public UpdateUserHandler UpdateUser() => new (_dbMock, _tokenMock, _validatorMock, _jobMock);

    [Fact]
    public async Task HandleAsync_DeveRetornarErroUpdateUserCommand_QuandoValidacaoFalhar()
    {
        var erros = new ValidationResult(new[]
        {
             new ValidationFailure("Id",  " obrigatório"),
             new ValidationFailure("Name",  "name invalido"),
             new ValidationFailure("Email", "Email inválido"),
             new ValidationFailure("Password", "Password invalidp")

        });

        _validatorMock
            .ValidateAsync(Arg.Any<UpdateUserCommand>())
            .Returns(erros);

        var updateUserCommand = new UpdateUserCommand(string.Empty, "na","@@","123");
        var result = await UpdateUser().HandleAsync(updateUserCommand);

        erros.Errors.Select(x => x.ErrorMessage)
            .SequenceEqual(result.Errors!.Select(e => e.Message))
            .Should()
            .BeTrue();

       await _dbMock.DidNotReceive()
          .SaveChangesAsync();
        await _jobMock.DidNotReceiveWithAnyArgs()
            .AddJobAsync(default!);
        await _tokenMock.DidNotReceiveWithAnyArgs()
            .CriarToken(default!);

    }

    [Fact]
    public async Task HandleAsync_DeveRetornarErro_NotFaund()
    {
        _validatorMock
             .ValidateAsync(Arg.Any<UpdateUserCommand>())
             .Returns(new ValidationResult());

        var updateUserCommand = new UpdateUserCommand(Guid.NewGuid().ToString(), string.Empty, string.Empty, string.Empty);
        _dbMock.Users.FindAsync(Arg.Any<Guid>())!.Returns(ValueTask.FromResult<Domain.Entity.User>(null));

        var result = await UpdateUser().HandleAsync(updateUserCommand);
        result.IsSuccess.Should().BeFalse();
        result.Errors!.Should().Contain(Error.NotFound);

       

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _jobMock.DidNotReceiveWithAnyArgs().AddJobAsync(default!);
        await _tokenMock.DidNotReceiveWithAnyArgs().CriarToken(default!);

    }

    [Fact]
    public async Task HandleAsync_DeveAtualizar_SemSenha()
    {
        var userId = Guid.NewGuid();
        var initialHash = BCrypt.Net.BCrypt.HashPassword("initial-hash");
        var user = new Domain.Entity
            .User(userId, DateTime.UtcNow, DateTime.UtcNow, "oldName", initialHash, "old@email");

        var command = new UpdateUserCommand(userId.ToString(), "newName", "new@email", string.Empty);

        _validatorMock
              .ValidateAsync(Arg.Any<UpdateUserCommand>())
              .Returns(new ValidationResult());

        _dbMock.Users.FindAsync(Arg.Any<Guid>())!
            .Returns(ValueTask.FromResult(user));

        _dbMock.SaveChangesAsync().Returns(Task.FromResult(1));
        _tokenMock.CriarToken(Arg.Any<Domain.Entity.User>())
            .Returns(Task.FromResult(new Token("tok")));

        _jobMock.AddJobAsync(Arg.Any<SyncData<Domain.Entity.User>>())
            .Returns(ValueTask.CompletedTask);

        var result = await UpdateUser().HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("newName");
        result.Value.Email.Should().Be("new@email");
        result.Value.Token.Should().Be("tok");
        user.PasswordHash.Should().Be(initialHash);

        await _dbMock.Received(1).SaveChangesAsync();
        await _tokenMock.Received(1).CriarToken(Arg.Any<Domain.Entity.User>());
        await _jobMock.ReceivedWithAnyArgs(1).AddJobAsync(default!);

    }

    [Fact]
    public async Task HandleAsync_DeveAtualizar_ComSenha()
    {
        var userId = Guid.NewGuid();
        var initialHash = BCrypt.Net.BCrypt.HashPassword("initial-hash");
        var newPassword = BCrypt.Net.BCrypt.HashPassword("UPDATE-hash");

        var user = new Domain.Entity
            .User(userId, DateTime.UtcNow, DateTime.UtcNow, "oldName", initialHash, "old@email");

        var command = new UpdateUserCommand(userId.ToString(), string.Empty, string.Empty, newPassword);

        _validatorMock
              .ValidateAsync(Arg.Any<UpdateUserCommand>())
              .Returns(new ValidationResult());

        _dbMock.Users.FindAsync(Arg.Any<Guid>())!
            .Returns(ValueTask.FromResult(user));

        _dbMock.SaveChangesAsync().Returns(Task.FromResult(1));
        _tokenMock.CriarToken(Arg.Any<Domain.Entity.User>())
            .Returns(Task.FromResult(new Token("tok")));

        _jobMock.AddJobAsync(Arg.Any<SyncData<Domain.Entity.User>>())
            .Returns(ValueTask.CompletedTask);

        var result = await UpdateUser().HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("oldName");
        result.Value.Email.Should().Be("old@email");
        result.Value.Token.Should().Be("tok");

        user.PasswordHash.Should().NotBe(initialHash);
        BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash).Should().BeTrue();

        await _dbMock.Received(1).SaveChangesAsync();
        await _tokenMock.Received(1).CriarToken(Arg.Any<Domain.Entity.User>());
        await _jobMock.ReceivedWithAnyArgs(1).AddJobAsync(default!);



    }



}
