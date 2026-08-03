using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Extensions;
using VibeTree.Features.User;
using VibeTree.Features.User.CreateUser.Commands;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using Xunit.Abstractions;
namespace VibeTree.Test.IntegrationTests.Users;

[Collection(IntegrationCollection.Name)]
public class CreateUserAndProfileEndpointTests(CustomWebApplicationFactory factory, ITestOutputHelper _output) : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();
    private readonly ITestOutputHelper _testOutputHelper = _output;

    private IHost Host => _factory.WolverineHost!;

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

        CreateUserAndProfileCommand command = new (
            faker.Person.FullName, 
            plainPassword, 
            faker.Person.Email,
            faker.Person.FullName
        );

        HttpResponseMessage response =await Utils.Cast(
            this.Host,
            Task.Run(async () => await _client.PostAsJsonAsync("/user", command))
        );

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

           
            var savedUserRead = await readDb.Users.Include(a=>a.Perfil)
            .FirstOrDefaultAsync(u => u.Id == result.Value.Id);
            _testOutputHelper.WriteLine(savedUserRead.ToJson());


            savedUserRead.Should().NotBeNull();
            savedUserRead!.Name.Should().Be(command.Name);
            BCrypt.Net.BCrypt.Verify(plainPassword, savedUserRead.PasswordHash).Should().BeTrue();
            savedUserRead.Perfil.IdUser.Should().Be(result.Value.Id);

            using var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            var savedUserWrite = await writeDb.Users.Include(a => a.Perfil)
            .FirstOrDefaultAsync(u => u.Id == result.Value.Id);

            savedUserWrite.Should().NotBeNull();
            savedUserWrite!.Name.Should().Be(command.Name);
            BCrypt.Net.BCrypt.Verify(plainPassword, savedUserWrite.PasswordHash).Should().BeTrue();
            savedUserWrite.Perfil.IdUser.Should().Be(result.Value.Id);



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