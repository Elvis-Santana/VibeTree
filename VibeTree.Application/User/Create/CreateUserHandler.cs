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
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IValidator<CreateUserCommand> _validator = validator;


    public async Task<Result<Userlogin>> HandleAsync(CreateUserCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);

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

        await _appDbContext.Users.AddAsync(user);
        await _appDbContext.SaveChangesAsync();

       Token token = await  _tokenService.CriarToken(user);

        return new Userlogin(user.Id, user.Name,user.Email, token.token);
          
    }
}
