using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using System.Linq.Expressions;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Common;
using VibeTree.Application.Sync;
using VibeTree.Application.User.Update;
using VibeTree.Domain.Entity;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.Unitario.Update;

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
            .CriarToken(default!, default!);

    }

    [Fact]
    public async Task HandleAsync_DeveRetornarErro_NotFaund()
    {
        _validatorMock
             .ValidateAsync(Arg.Any<UpdateUserCommand>())
             .Returns(new ValidationResult());

        var listaVazia = new List<Domain.Entity.User>();
        var mockDbSet = listaVazia.BuildMockDbSet();
        _dbMock.Users.Returns(mockDbSet);

        var updateUserCommand = new UpdateUserCommand(Guid.NewGuid().ToString(), string.Empty, string.Empty, string.Empty);
   

        var result = await UpdateUser().HandleAsync(updateUserCommand);
        result.IsSuccess.Should().BeFalse();
        result.Errors!.Should().Contain(Error.NotFound);

       

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _jobMock.DidNotReceiveWithAnyArgs().AddJobAsync(default!);
        await _tokenMock.DidNotReceiveWithAnyArgs().CriarToken(default!, default!);

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

        SetupUserAndPerfilMocks(user);

        _dbMock.SaveChangesAsync().Returns(Task.FromResult(1));

        _tokenMock.CriarToken(Arg.Any<Domain.Entity.User>(), Arg.Any<Domain.Entity.Perfil>())
            .Returns(Task.FromResult(new Token(string.Empty)));

        _jobMock.AddJobAsync(Arg.Any<SyncData<Domain.Entity.User>>())
            .Returns(ValueTask.CompletedTask);

        var result = await UpdateUser().HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("newName");
        result.Value.Email.Should().Be("new@email");
        result.Value.Token.Should().BeNullOrEmpty();
        user.PasswordHash.Should().Be(initialHash);

        await _dbMock.Received(1).SaveChangesAsync();
        await _tokenMock.DidNotReceive().CriarToken(default!, default!);
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

        SetupUserAndPerfilMocks(user);


        _validatorMock
              .ValidateAsync(Arg.Any<UpdateUserCommand>())
              .Returns(new ValidationResult());

        _dbMock.SaveChangesAsync().Returns(Task.FromResult(1));
        _tokenMock.CriarToken(Arg.Any<Domain.Entity.User>(), Arg.Any<Domain.Entity.Perfil>())
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
        await _tokenMock.Received(1).CriarToken(Arg.Any<Domain.Entity.User>(), Arg.Any<Domain.Entity.Perfil>());
        await _jobMock.ReceivedWithAnyArgs(1).AddJobAsync(default!);



    }

    private void SetupUserAndPerfilMocks(Domain.Entity.User user)
    {
        var listaUser = new List<Domain.Entity.User>() { user };
        var mockDbSetUser = listaUser.BuildMockDbSet();
        _dbMock.Users.Returns(mockDbSetUser);

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

        var listaPerfil = new List<Domain.Entity.Perfil>() { perfil };
        var mockDbSetPerfil = listaPerfil.BuildMockDbSet();
        _dbMock.perfils.Returns(mockDbSetPerfil);
    }


}
