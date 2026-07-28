using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlX.XDevAPI;
using System.Net;
using System.Net.Http.Json;
using VibeTree.Features.User;
using VibeTree.Features.User.Update.Commands;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using VibeTree.Shared.Entity;

namespace VibeTree.Test.UserTests.Integracao.Update;

[Collection(IntegrationCollection.Name)]
public class UpdateUserHandlerIntegracao(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly HttpClient client = factory.CreateClient();
    private readonly CustomWebApplicationFactory _factory = factory;
    private IHost Host => _factory.WolverineHost!;

    public async Task InitializeAsync()=> await this.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

        await ClearDataAsync(writeDb);
        await ClearDataAsync(readDb);
    }
    private  async Task ClearDataAsync(DbContext db)
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task UpdateUserHandler_Should_Not_Found_User()
    {
      
        UpdateUserCommand user = new Faker<UpdateUserCommand>("pt_BR")
        .CustomInstantiator(f => new(
            Guid.NewGuid().ToString(),
            string.Empty,
            string.Empty,
            string.Empty
            )
        ).Generate();

        HttpResponseMessage response = await Utils.Cast(
            this.Host, 
            Task.Run(async () =>await client.PatchAsJsonAsync<UpdateUserCommand>("/user", user))
        );


        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.IsSuccessStatusCode.Should().BeFalse();
        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();

        result.IsSuccess.Should().BeFalse();


      
            using var scope = _factory.Services.CreateScope();
            using var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            (await readDb.Users.FindAsync(Guid.Parse(user.Id))).Should().BeNull();
    
    }

    [Fact]
    public async Task UpdateUserHandler_Should_Update_Sem_Senha_User()
    {
        Guid id = Guid.NewGuid();
        User user = new Faker<User>("pt_BR")
        .CustomInstantiator(f => new(
            id,
            DateTime.UtcNow,
            DateTime.UtcNow,
            f.Internet.UserName(),
            BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()),
            f.Internet.Email()
            )
        ).Generate();


        using (var scope = _factory.Services.CreateScope())
        {
            using var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
                await writeDb.Users.AddAsync(user);
                await writeDb.SaveChangesAsync();

            using var readbSeed = scope.ServiceProvider.GetRequiredService<ReadDbContext>(); 
                await readbSeed.Users.AddAsync(user);
                await readbSeed.SaveChangesAsync();       
        }

        UpdateUserCommand updateUser = new Faker<UpdateUserCommand>("pt_BR")
        .CustomInstantiator(f => new UpdateUserCommand(
            id.ToString(),
            f.Internet.UserName(),
            f.Internet.Email(),
            string.Empty)
        ).Generate();


        HttpResponseMessage response = await Utils.Cast(
                 this.Host,
                 Task.Run(async () => await client.PatchAsJsonAsync<UpdateUserCommand>("/user", updateUser))
             );
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(updateUser.Name);
        result.Value.Email.Should().Be(updateUser.Email);
        result.Value.Token.Should().BeNullOrWhiteSpace();

       
        using (var scope = _factory.Services.CreateScope())
        {
            var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            var u = await readDb.Users.FindAsync(id);
            u.Should().NotBeNull();
            u.Email.Should().Be(updateUser.Email);
            u.Name.Should().Be(updateUser.Name);
        }
       
    }

    [Fact]
    public async Task UpdateUserHandler_Should_Update_Com_Senha_User()
    {
        Guid id = Guid.NewGuid();
        User user = new Faker<User>("pt_BR")
        .CustomInstantiator(f => new(
            id,
            DateTime.UtcNow,
            DateTime.UtcNow,
            f.Internet.UserName(),
            BCrypt.Net.BCrypt.HashPassword(f.Internet.Password()),
            f.Internet.Email()
            )
        ).Generate();

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

        using (var scope = _factory.Services.CreateScope())
        {
            using var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
            await writeDb.Users.AddAsync(user);
            await writeDb.perfils.AddAsync(perfil);
            await writeDb.SaveChangesAsync();

            using var readbSeed = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            await readbSeed.Users.AddAsync(user);
            await readbSeed.perfils.AddAsync(perfil);
            await readbSeed.SaveChangesAsync();
        }




        UpdateUserCommand updateUser = new Faker<UpdateUserCommand>("pt_BR")
        .CustomInstantiator(f => new UpdateUserCommand(
            id.ToString(),
            f.Internet.UserName(),
            f.Internet.Email(),
            f.Internet.Password())
        ).Generate();




        HttpResponseMessage response = await Utils.Cast(
                 this.Host,
                 Task.Run(async () => await client.PatchAsJsonAsync<UpdateUserCommand>("/user", updateUser))
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessStatusCode.Should().BeTrue();
        var result = await response.Content.ReadFromJsonAsync<Result<Userlogin>>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(updateUser.Name);
        result.Value.Email.Should().Be(updateUser.Email);


        using (var scope = _factory.Services.CreateAsyncScope())
        {
            var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            var u = await readDb.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(user => user.Id == id);
            u.Should().NotBeNull();
            u.Email.Should().Be(updateUser.Email);
            u.Name.Should().Be(updateUser.Name);
            BCrypt.Net.BCrypt.Verify(updateUser.Password, u.PasswordHash).Should().BeTrue();
        }
      
    }


}
