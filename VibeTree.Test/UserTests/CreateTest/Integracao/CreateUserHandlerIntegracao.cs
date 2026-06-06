using Bogus;
using FluentAssertions;
using NSubstitute.Extensions;
using System.Threading;
using VibeTree.Application;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.QuerySync;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Domain;
using VibeTree.Infrastructure.Query;
using VibeTree.User.CreateUser;
namespace VibeTree.Test.UserTests.CreateTest.Integracao;

public class CreateUserHandlerIntegracao 
{

  

    [Fact]
    public async Task CreateUserHandler_Should_Create_User()
    {

        await using var db = new DbContextBuildConfig();
        var context = await db.CriarContextoWritePreparadoAsync();


        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        CreateUserCommand user = new Faker<CreateUserCommand>("pt_BR")
        .CustomInstantiator(f =>
        new(f.Person.FullName, f.Internet.Password(), f.Person.Email)).Generate();

        IQueryJobSynchronize<SyncData<Domain.Entity.User>> queryJobSynchronize = new QueryJobSynchronizeUserDb();
        IMediator mediator = FactoryMed
            .CreateMediatorWithHandler(new CreateUserHandler(context, service, new CreateUserValidator(), queryJobSynchronize));


        Result<Userlogin> result = await mediator.SendAsync(user);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(user.Name);
        result.Value.Email.Should().Be(user.Email);
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();


        queryJobSynchronize.HasPending.Should().BeTrue();
       await foreach (var job in queryJobSynchronize.ReadAllAsync())
       {
            job.Operation.Should().Be(SyncOperation.Create);
            job.Item.Id.Should().Be(result.Value.Id);
            BCrypt.Net.BCrypt.Verify(user.Password, job.Item.PasswordHash).Should().BeTrue();

            break;
       }


    }

    [Fact]
    public async Task CreateUserHandler_Should_Return_Error_When_Invalid_Data()
    {
        await using var db = new DbContextBuildConfig();
        IWriteDbContext context = await db.CriarContextoWritePreparadoAsync();

        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        CreateUserCommand user = new(string.Empty, string.Empty, string.Empty);
        IQueryJobSynchronize<SyncData<Domain.Entity.User>> queryJobSynchronize = new QueryJobSynchronizeUserDb();

        IMediator mediator = FactoryMed.CreateMediatorWithHandler(new CreateUserHandler(context, service, new CreateUserValidator(), queryJobSynchronize));


        Result<Userlogin> result = await mediator.SendAsync(user);
        result.Should().NotBeNull();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Contains(Error.EmailAddress).Should().BeTrue();
        result.Errors.Contains(Error.EmailEmpty).Should().BeTrue();

        result.Errors.Contains(Error.NameMinimumLength).Should().BeTrue();
        result.Errors.Contains(Error.NameEmpty).Should().BeTrue();

        result.Errors.Contains(Error.PasswordEmpty).Should().BeTrue();
        result.Errors.Contains(Error.PasswordMinimumLength).Should().BeTrue();

        queryJobSynchronize.HasPending.Should().BeFalse();




    }
}