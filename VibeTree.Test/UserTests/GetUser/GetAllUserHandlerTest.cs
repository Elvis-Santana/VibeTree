using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User;
using VibeTree.Application.User.Get;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.User.CreateUser;

namespace VibeTree.Test.UserTests.GetUser;

public class GetAllUserHandlerTest
{

    private readonly AppDbContext _context;

    public GetAllUserHandlerTest()
    {
        var option =  new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

        _context = new (option);
    }

    [Fact]
    public async Task GetAllUserHandler_Should_Return_UserResponse()
    {
        var faker = new Faker<Domain.Entity.User>("pt_BR")
            .CustomInstantiator(f => new Domain.Entity.User(
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                f.Name.FullName(),
                f.Internet.Email(),
                f.Internet.Password()
            ));

        var users = faker.Generate(5);

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        IHandler<GetAllUserQuery, List<UserResponse>> queryHandler = new GetAllUserHandler(_context);

        List<UserResponse> result = await queryHandler
            .HandleAsync(new GetAllUserQuery());

        result.Should().NotBeNull();
        result.Should().BeOfType<List<UserResponse>>();
        result.Count.Should().Be(5);
    }

}
