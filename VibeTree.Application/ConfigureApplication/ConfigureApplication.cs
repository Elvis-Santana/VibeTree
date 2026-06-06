using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Perfil;
using VibeTree.Application.Perfil.Create;
using VibeTree.Application.Perfil.Get.GetById;
using VibeTree.Application.Perfil.Get.GetBySlug;
using VibeTree.Application.User;
using VibeTree.Application.User.Login;
using VibeTree.Application.User.Update;
using VibeTree.User.CreateUser;
namespace VibeTree.Application.ConfigureApplication;

public static class ConfigureApplication
{

    public static void ConfigureServicesApplication(this IServiceCollection services, IConfiguration configuration)
    {

        var jwt = configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["secret_key"]));

        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    //ValidAudience = jwt[Audience],
                    IssuerSigningKey = secretKey
                };
            });

        services.AddAuthorization();

      

        services.AddScoped<ITokenService, TokenService>();
        services.AddTransient<IHandler<CreateUserCommand, Userlogin>, CreateUserHandler>();
        //services.AddTransient<IHandler<GetAllUserQuery, List<Userlogin>>, GetAllUserHandler>();
        services.AddTransient<IHandler<LoginQuery, Userlogin>, LoginHandler>();

        services.AddTransient<IHandler<CreatePerfilCommand,PerfilResponse>,  CreatePerfilHandler>();
        services.AddTransient<IHandler<GetPerfilByIdQuery, PerfilResponse>, GetPerfilByIdHandler>();
        services.AddTransient<IHandler<GetPerfilBySlugQuery, PerfilResponse>, GetPerfilBySlugHandler>();

        services.AddScoped<IValidator<LoginQuery>, LoginValidator>();
        services.AddScoped<IValidator<CreatePerfilCommand>, CreatePerfilValidator>();
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserValidator>();
        services.AddScoped<IValidator<UpdateUserCommand>, UpdateUserValidator>();




    }
}

