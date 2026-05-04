using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Application.User.Login;

public class LoginHandler(
    AppDbContext appDbContext,
    ITokenService tokenService,
    IValidator<LoginQuery> validator
    ) : IHandler<LoginQuery, Userlogin>
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IValidator<LoginQuery> _validator = validator;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<Userlogin> HandleAsync(LoginQuery query)
    {

        Domain.Entity.User? user = await this._appDbContext.Users
            .FirstOrDefaultAsync(u => u.Email.Equals(query.email));

        var validationResult = _validator.Validate(query);

        if (!validationResult.IsValid )
            return null;

        Token token = await this._tokenService.CriarToken(user!);
        return new Userlogin(user!.Id, user.Name, user.Email, token.token);

    }
}


