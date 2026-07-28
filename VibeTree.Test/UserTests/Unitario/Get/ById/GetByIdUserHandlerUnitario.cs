using Bogus;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Common;
using VibeTree.Application.User;
using VibeTree.Application.User.Get.ById;
using VibeTree.Entity;

namespace VibeTree.Test.UserTests.Unitario.Get.ById;

public class GetByIdUserHandlerUnitario
{
    private IReadDbContext readDbContextMock = Substitute.For<IReadDbContext>();

    [Fact]
    public async Task Shoud_Retornar_Error_Nao_Encontrado()
    {
        readDbContextMock.Users.FindAsync(Arg.Any<Guid>()).Returns(ValueTask.FromResult<User>(null));

        GetByIdUserQuery getByIdUserQuery = new(Guid.NewGuid().ToString());
        GetByIdUserHandler handler = new (readDbContextMock);


        Result<UserResponse> result =   await handler.HandleAsync(getByIdUserQuery);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Be(Error.UserNotFound);

        await readDbContextMock.Users.Received().FindAsync(Arg.Any<Guid>());

    }

    [Fact]
    public async Task Shoud_Retornar_Usuario_Id_Valido()
    {
        var user = new Faker<User>("pt_BR")
            .CustomInstantiator(u => new(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                u.Internet.UserName(),
                u.Internet.Password(),
                u.Internet.Email()
                )
            ).Generate();

         readDbContextMock.Users.FindAsync(Arg.Any<Guid>()).Returns(ValueTask.FromResult<User>(user));

        GetByIdUserQuery getByIdUserQuery = new(user.Id.ToString());
        GetByIdUserHandler handler = new(readDbContextMock);


        Result<UserResponse> result = await handler.HandleAsync(getByIdUserQuery);

        result.IsSuccess.Should().BeTrue();

        UserResponse userResponse = new (user.Id, user.Name, user.Email);
        result.Value.Should().Be(userResponse);

        await readDbContextMock.Users.Received().FindAsync(Arg.Any<Guid>());

    }

    [Fact]
    public async Task Shoud_Retornar_Error_Id_Vazio()
    {
        readDbContextMock.Users.FindAsync(Arg.Any<Guid>())!.Returns(ValueTask.FromResult<User>(null));

        GetByIdUserQuery getByIdUserQuery = new("");
        GetByIdUserHandler handler = new(readDbContextMock);


        Result<UserResponse> result = await handler.HandleAsync(getByIdUserQuery);

        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Should().Be(Error.IdValid);

        await readDbContextMock.Users.DidNotReceive().FindAsync(Arg.Any<Guid>());

    }
}
