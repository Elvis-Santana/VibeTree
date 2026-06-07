using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.QuerySync;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Application.User.Update;
using VibeTree.Infrastructure.Query;
using VibeTree.User.CreateUser;

namespace VibeTree.Test.UserTests.CreateTest.Integracao;

public class UpdateUserHandlerIntegracao  
{

    [Fact]
    public async Task UpdateUserHandler_Should_Not_Found_User()
    {
        await using var db = new DbContextBuildConfig();
        var context = await db.CriarContextoWritePreparadoAsync();

        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        UpdateUserCommand user = new Faker<UpdateUserCommand>("pt_BR")
        .CustomInstantiator(f =>  new (
            Guid.NewGuid().ToString(),
            string.Empty,
            string.Empty, 
            string.Empty
            )
        ).Generate();

        IQueryJobSynchronize<SyncData<Domain.Entity.User>> queryJobSynchronize = new QueryJobSynchronizeUserDb();

        IMediator mediator = FactoryMed
        .CreateMediatorWithHandler(
            new UpdateUserHandler(context, service, new UpdateUserValidator(), 
            queryJobSynchronize)
        );


        Result<Userlogin> result = await mediator.SendAsync(user);
        result.IsSuccess.Should().BeFalse();


    }

    [Fact]
    public async Task UpdateUserHandler_Should_Update_User()
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

        var config = JwtBuildConfig.BuildConfig();
        var service = new TokenService(config);

        UpdateUserCommand updateUser = new Faker<UpdateUserCommand>("pt_BR")
        .CustomInstantiator(f => new (
            id.ToString(),
            f.Internet.UserName(),
            f.Internet.Email(),
            f.Internet.Password())
        ).Generate();

        IQueryJobSynchronize<SyncData<Domain.Entity.User>> queryJobSynchronize = new QueryJobSynchronizeUserDb();

        IMediator mediator = FactoryMed
        .CreateMediatorWithHandler(
            new UpdateUserHandler(context, service, new UpdateUserValidator(), 
            queryJobSynchronize)
        );


        Result<Userlogin> result = await mediator.SendAsync(updateUser);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(updateUser.Name);
        result.Value.Email.Should().Be(updateUser.Email);
        result.Value.Token.Should().NotBeNullOrWhiteSpace();
        BCrypt.Net.BCrypt.Verify(updateUser.Password, user.PasswordHash).Should().BeTrue();

        queryJobSynchronize.HasPending.Should().BeTrue();

 
    }
}
