using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.User.CreateUser;

public class CreateUserHandler (AppDbContext appDbContext, ITokenService tokenService) :IHandler<CreateUserCommand, Userlogin>
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly ITokenService _tokenService = tokenService;


    public async Task<Userlogin> HandleAsync(CreateUserCommand command)
    {
        Domain.Entity.User user = new (
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            command.name,
            BCrypt.Net.BCrypt.HashPassword(command.password),
            command.email
        );

        await _appDbContext.Users.AddAsync(user);
        await _appDbContext.SaveChangesAsync();

       Token token = await  _tokenService.CriarToken(user);

        return new Userlogin(user.Id, user.Name,user.Email, token.token);
          
    }
}
