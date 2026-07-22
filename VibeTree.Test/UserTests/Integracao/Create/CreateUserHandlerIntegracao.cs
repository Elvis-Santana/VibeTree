using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Application.Common;
using VibeTree.Application.User;
using VibeTree.Application.User.Create.Commands;
using VibeTree.Infrastructure.AppDbContext;
namespace VibeTree.Test.UserTests.Integracao.Create;

[Collection(IntegrationCollection.Name)]
public class CreateUserHandlerIntegracao(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        await ClearDataAsync(writeDb);
        await ClearDataAsync(readDb);
    }

    private static async Task ClearDataAsync(DbContext db)
    {

        if (db is WriteDbContext writeDb)
        {
            writeDb.Users.RemoveRange(writeDb.Users);
            await writeDb.SaveChangesAsync();
        }
        else if (db is ReadDbContext readDb)
        {
            readDb.Users.RemoveRange(readDb.Users);
            await readDb.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Deve_Criar_Usuario_e_Perfil_Com_Sucesso()
    {
        var faker = new Faker("pt_BR");
        var plainPassword = faker.Internet.Password();
        var command = new CreateUserAndProfileCommand(faker.Person.FullName, plainPassword, faker.Person.Email, faker.Person.FullName);

        var response = await _client.PostAsJsonAsync("/user", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Email.Should().Be(command.Email);
        result.Value.Token.Should().NotBeNullOrWhiteSpace();

        using (var scope = _factory.Services.CreateScope())
        {
            using var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

                var savedUserRead = await readDb.Users
                .FirstOrDefaultAsync(u => u.Id == result.Value.Id);

                savedUserRead.Should().NotBeNull();
                savedUserRead!.Name.Should().Be(command.Name);
                BCrypt.Net.BCrypt.Verify(plainPassword, savedUserRead.PasswordHash).Should().BeTrue();

            using var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
                var savedUserWrite = await writeDb.Users
                .FirstOrDefaultAsync(u => u.Id == result.Value.Id);

                savedUserWrite.Should().NotBeNull();
                savedUserWrite!.Name.Should().Be(command.Name);
                BCrypt.Net.BCrypt.Verify(plainPassword, savedUserWrite.PasswordHash).Should().BeTrue();
            
        }
       

    }

    [Fact]
    public async Task Deve_Retornar_BadRequest_Por_Informacores_Invalidas()
    {
        CreateUserAndProfileCommand command = new(string.Empty, string.Empty, string.Empty, string.Empty);

        var response = await _client.PostAsJsonAsync("/user", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.IsSuccessStatusCode.Should().BeFalse();

        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();

        result!.IsSuccess.Should().BeFalse();
        result.Errors!.Contains(Error.EmailAddress).Should().BeTrue();
        result.Errors.Contains(Error.EmailEmpty).Should().BeTrue();
        result.Errors.Contains(Error.NameMinimumLength).Should().BeTrue();
        result.Errors.Contains(Error.NameEmpty).Should().BeTrue();
        result.Errors.Contains(Error.PasswordEmpty).Should().BeTrue();
        result.Errors.Contains(Error.PasswordMinimumLength).Should().BeTrue();
    }

}