using FluentValidation;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Application.User;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.User.CreateUser;

public class CreateUserHandler (
    AppDbContext appDbContext,
    ITokenService tokenService,
    IValidator<CreateUserCommand> validator
    ) :IHandler<CreateUserCommand, Userlogin>
{


    public async Task<Result<Userlogin>> HandleAsync(CreateUserCommand command)
    {
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();
        

        Domain.Entity.User user = new (
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            command.Name,
            BCrypt.Net.BCrypt.HashPassword(command.Password),
            command.Email
        );

        await appDbContext.Users.AddAsync(user);
        await appDbContext.SaveChangesAsync();

       Token token = await tokenService.CriarToken(user);

        return new Userlogin(user.Id, user.Name,user.Email, token.token);
          
    }
}
