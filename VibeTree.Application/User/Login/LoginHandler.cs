using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Infrastructure.AppDbContext;
using BCryptNet = BCrypt.Net.BCrypt;
namespace VibeTree.Application.User.Login;

public class LoginHandler(
    AppDbContext appDbContext,
    ITokenService tokenService,
    IValidator<LoginQuery> validator
    ) : IHandler<LoginQuery, Userlogin>
{

    public async Task<Result<Userlogin>> HandleAsync(LoginQuery query)
    {

        var validationResult = validator.Validate(query);

        if (!validationResult.IsValid)
            return validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();

        Domain.Entity.User? user = await appDbContext
            .Users
            .FirstOrDefaultAsync(u => u.Email.Equals(query.email));

        if (user is null || !(BCryptNet.Verify(query.password, user?.PasswordHash)))
            return Error.InvalidCredentials;


        Token token = await tokenService.CriarToken(user!);

        return new Userlogin(user!.Id, user.Name, user.Email, token.token);

    }
}


