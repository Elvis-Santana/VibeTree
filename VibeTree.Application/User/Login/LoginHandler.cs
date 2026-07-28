using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Common;
using BCryptNet = BCrypt.Net.BCrypt;
namespace VibeTree.Application.User.Login;

public class LoginHandler(
    IReadDbContext appDbContext,
    ITokenService tokenService,
    IValidator<LoginQuery> validator
    ){

    public async Task<Result<Userlogin>> Handle(LoginQuery query)
    {

        var validationResult =await  validator.ValidateAsync(query);

        if (!validationResult.IsValid)
            return validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();

        Domain.Entity.User? user = await appDbContext
            .Users.Include(a => a.Perfil)
            .FirstOrDefaultAsync(u => u.Email.Equals(query.email));

        if (user is null || !(BCryptNet.Verify(query.password, user?.PasswordHash)))
            return Error.UserNotFound;


        Token token = await tokenService.CriarToken(user!, user!.Perfil);

        return new Userlogin(user!.Id, user.Name, user.Email, token.token);

    }
}


