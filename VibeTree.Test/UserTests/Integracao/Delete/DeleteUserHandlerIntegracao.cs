using Bogus;
using FluentAssertions;
using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Application.User.Delete;
using VibeTree.Infrastructure.Query;

namespace VibeTree.Test.UserTests.Integracao.Delete;


public class DeleteUserHandlerIntegracao 
{
   

    [Fact]
    public async Task DeleteUserHandler_Should_Not_Found_User()
    {
        await using var db = new DbContextBuildConfig();
        var context = await db.CriarContextoWritePreparadoAsync();

        DeleteUserCommand command = new DeleteUserCommand(Guid.NewGuid().ToString());

        IQueueSynchronizeDb<SyncData<Domain.Entity.User>> queryJobSynchronize = new QueueSynchronize<Domain.Entity.User>();

        IMediator mediator = FactoryMed
        .CreateMediatorWithHandler(
            new DeleteUserHandler(context,  new DeleteUserValidator(),  queryJobSynchronize)
        );


        Result<bool> result = await mediator.SendAsync(command);
        result.IsSuccess.Should().BeFalse();

        queryJobSynchronize.HasPending.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserHandler_Should_Delete_User()
    {
        await using var db = new DbContextBuildConfig();
        var context = await db.CriarContextoWritePreparadoAsync();

        Guid id = Guid.NewGuid();
        Domain.Entity.User user = new Faker<Domain.Entity.User>("pt_BR")
        .CustomInstantiator(f => new(
            id,
            DateTime.UtcNow,
            DateTime.UtcNow,
            f.Internet.UserName(),
            BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()),
            f.Internet.Email()
            )
        ).Generate();

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        DeleteUserCommand command = new DeleteUserCommand(id.ToString());

        IQueueSynchronizeDb<SyncData<Domain.Entity.User>> queryJobSynchronize = new QueueSynchronize<Domain.Entity.User>();

        IMediator mediator = FactoryMed
        .CreateMediatorWithHandler(
            new DeleteUserHandler(context, new DeleteUserValidator(), queryJobSynchronize)
        );


        Result<bool> result = await mediator.SendAsync(command);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        queryJobSynchronize.HasPending.Should().BeTrue();

    }
}
