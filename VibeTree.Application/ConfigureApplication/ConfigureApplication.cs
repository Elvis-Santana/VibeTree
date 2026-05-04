using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User;
using VibeTree.Application.User.Get;
using VibeTree.Application.User.Login;
using VibeTree.User.CreateUser;

namespace VibeTree.Application.ConfigureApplication;

public static class ConfigureApplication
{

    public static void ConfigureServicesApplication(this IServiceCollection services, IConfiguration configuration)
    {

        var jwt = configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["secret_key"]));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    //ValidAudience = jwt[Audience],
                    IssuerSigningKey = secretKey
                };
            });

        services.AddScoped<ITokenService, TokenService>();
        services.AddTransient<IHandler<CreateUserCommand, Userlogin>, CreateUserHandler>();
        services.AddTransient<IHandler<GetAllUserQuery, List<Userlogin>>, GetAllUserHandler>();
        services.AddTransient<IHandler<LoginQuery, Userlogin>, LoginHandler>();

        services.AddScoped<IValidator<LoginQuery>, LoginValidator>();




    }
}
