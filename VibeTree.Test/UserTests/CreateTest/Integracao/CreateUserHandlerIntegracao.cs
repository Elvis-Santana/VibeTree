using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Infrastructure.AppDbContext;
using VibeTree.User.CreateUser;
namespace VibeTree.Test.UserTests.CreateTest.Integracao;

public class CreateUserHandlerIntegracao(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient client = factory.CreateClient();
    private readonly IServiceProvider services = factory.Services;

    [Fact]
    public async Task Deve_Criar_Usuario_Com_Sucesso()
    {
        // Arrange

        using (var scope = services.CreateScope())
        {
            var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await writeDb.Database.EnsureDeletedAsync();
            await writeDb.Database.EnsureCreatedAsync();

            var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await readDb.Database.EnsureDeletedAsync();
            await readDb.Database.EnsureCreatedAsync();
        }



        CreateUserCommand command = new Faker<CreateUserCommand>("pt_BR")
            .CustomInstantiator(f =>
                new CreateUserCommand(
                    f.Person.FullName,
                    f.Internet.Password(),
                    f.Person.Email))
            .Generate();

        // Act

        var response =
            await client.PostAsJsonAsync<CreateUserCommand>(
                "/user",
                command);


        //assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = services.CreateScope())
        {
            var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            var usuarioSalvo = await readDb.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
            usuarioSalvo.Should().NotBeNull();
        }
    }



    [Fact]
    public async Task Deve_Retornar_BadRequest_Por_Informacores_Invalidas()
    {
        using (var scope = services.CreateScope())
        {
            var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await writeDb.Database.EnsureDeletedAsync();
            await writeDb.Database.EnsureCreatedAsync();

            var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await readDb.Database.EnsureDeletedAsync();
            await readDb.Database.EnsureCreatedAsync();
        }

        CreateUserCommand command = new(string.Empty, string.Empty, string.Empty);
  
        var response = await client.PostAsJsonAsync<CreateUserCommand>("/user",command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.IsSuccessStatusCode.Should().BeFalse();

        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Contains(Error.EmailAddress).Should().BeTrue();
        result.Errors.Contains(Error.EmailEmpty).Should().BeTrue();

        result.Errors.Contains(Error.NameMinimumLength).Should().BeTrue();
        result.Errors.Contains(Error.NameEmpty).Should().BeTrue();

        result.Errors.Contains(Error.PasswordEmpty).Should().BeTrue();
        result.Errors.Contains(Error.PasswordMinimumLength).Should().BeTrue();

    }
}