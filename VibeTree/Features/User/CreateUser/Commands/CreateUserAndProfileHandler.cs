using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeTree.Features.User.CreateUser.Events;
using VibeTree.Shared.Auth;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.User.CreateUser.Commands;

public static class CreateUserAndProfileHandler {

    public static async Task<(Result<Userlogin> , SyncUserPerfilCreatedEvent?)> Handle(
        CreateUserAndProfileCommand command,
        WriteDbContext writeDbContext,
        ITokenService tokenService,
        IValidator<CreateUserAndProfileCommand> validator
    )
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var err = validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();
            return (err,null);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

        bool existe = await writeDbContext
            .Users
            .AsNoTracking()
            .AnyAsync(u=> u.Email==command.Email);

        if (existe)
            return (Error.ExisteUser,null);

           Shared.Entity.User user = new(
                     Guid.NewGuid(),
                     DateTime.UtcNow,
                     DateTime.UtcNow,
                     command.Name,
                      passwordHash,
                     command.Email
            );


        Shared.Entity.Perfil perfil = new(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    command.Slug,
                    user.Id
            );

            await writeDbContext.Users.AddAsync(user);
            await writeDbContext.perfils.AddAsync(perfil);

            await writeDbContext.SaveChangesAsync();

            var token = await tokenService.CriarToken(user, perfil);
            var @event = new SyncUserPerfilCreatedEvent(user, perfil);
            return (new Userlogin(user.Id, user.Name, user.Email, token.token), @event);

       
    }

}
