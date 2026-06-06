using Bogus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.QuerySync;
using VibeTree.Application.User.Update;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Test.UserTests.CreateTest.Unitario;

public class UpdateUserHandlerUnitario
{
    private readonly IWriteDbContext _dbMock = Substitute.For<IWriteDbContext>();
    private readonly ITokenService _tokenMock = Substitute.For<ITokenService>();

    private readonly IQueryJobSynchronize<SyncData<Domain.Entity.User>> _jobMock = Substitute.For<IQueryJobSynchronize<SyncData<Domain.Entity.User>>>();
    private readonly IValidator<UpdateUserCommand> _validatorMock = Substitute.For<IValidator<UpdateUserCommand>>();

    public UpdateUserHandler UpdateUser() => 
        new UpdateUserHandler(_dbMock, _tokenMock, _validatorMock, _jobMock);

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

        var createUserCommand = new UpdateUserCommand("", "na","@@","123");
        var result = await UpdateUser().HandleAsync(createUserCommand);

        await _dbMock.DidNotReceive()
            .SaveChangesAsync();
        await _jobMock.DidNotReceiveWithAnyArgs()
            .AddJob(default!);
        await _tokenMock.DidNotReceiveWithAnyArgs()
            .CriarToken(default!);

        for (int i = 0; i < erros.Errors.Count(); i++)
            erros.Errors[i].ErrorMessage.Should().Be(result.Errors[i].Message);
        


    }

    [Fact]
    public async Task HandleAsync_DeveRetornarErro_notFaund()
    {
        _validatorMock
             .ValidateAsync(Arg.Any<UpdateUserCommand>())
             .Returns(new ValidationResult());

        var createUserCommand = new UpdateUserCommand(Guid.NewGuid().ToString(), "", "", "");
        _dbMock.Users.FindAsync(Arg.Any<Guid>()).Returns(ValueTask.FromResult<Domain.Entity.User>(null));

        var result = await UpdateUser().HandleAsync(createUserCommand);

        await _dbMock.DidNotReceive().SaveChangesAsync();
        await _jobMock.DidNotReceiveWithAnyArgs().AddJob(default!);
        await _tokenMock.DidNotReceiveWithAnyArgs().CriarToken(default!);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Message.Should().Contain("not faund");
    }


}
