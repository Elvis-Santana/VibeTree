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
    ) : IHandler<LoginQuery, Result<Userlogin>>
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IValidator<LoginQuery> _valiator = validator;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<Result<Userlogin>> HandleAsync(LoginQuery query)
    {

        var validationResult = _valiator.Validate(query);

        if (!validationResult.IsValid)
            return validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();

        Domain.Entity.User? user = await this._appDbContext
            .Users
            .FirstOrDefaultAsync(u => u.Email.Equals(query.email));


        if (user is null || !(BCryptNet.Verify(query.password, user?.PasswordHash)))
            return Error.InvalidCredentials;

        Token token = await this._tokenService.CriarToken(user!);

        return new Userlogin(user!.Id, user.Name, user.Email, token.token);

    }
}


