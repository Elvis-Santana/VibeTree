using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VibeTree.User.CreateUser;

namespace VibeTree.Application.Auth;

public class TokenService : ITokenService
{
    private  readonly string _secretKey;

    public TokenService(IConfiguration  configuration)
    {
        const string key = "JwtSettings:secret_key";

        _secretKey =configuration[key]
            ?? throw new ("A chave secreta não foi encontrada no appsettings.json");
    }


    public Task<Token> CriarToken(Domain.Entity.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.NameIdentifier,user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };


        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Task.FromResult(new Token(tokenHandler.WriteToken(token)));
    }
}
