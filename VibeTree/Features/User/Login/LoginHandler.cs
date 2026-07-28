using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeTree.Shared.Auth;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using BCryptNet = BCrypt.Net.BCrypt;
namespace VibeTree.Features.User.Login;

public class LoginHandler(
    ReadDbContext appDbContext,
    ITokenService tokenService,
    IValidator<LoginQuery> validator
    ){

    public async Task<Result<Userlogin>> Handle(LoginQuery query)
    {

        var validationResult =await  validator.ValidateAsync(query);

        if (!validationResult.IsValid)
            return validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();

        Shared.Entity.User? user = await appDbContext
            .Users.Include(a => a.Perfil)
            .FirstOrDefaultAsync(u => u.Email.Equals(query.email));

        if (user is null || !(BCryptNet.Verify(query.password, user?.PasswordHash)))
            return Error.UserNotFound;


        Token token = await tokenService.CriarToken(user!, user!.Perfil);

        return new Userlogin(user!.Id, user.Name, user.Email, token.token);

    }
}


