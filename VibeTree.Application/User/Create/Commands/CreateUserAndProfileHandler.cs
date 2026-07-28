using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeTree.Application.Auth;
using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User.Create.Events;

namespace VibeTree.Application.User.Create.Commands;

public static class CreateUserAndProfileHandler {

    public static async Task<(Result<Userlogin> , SyncUserPerfilCreatedEvent?)> Handle(
        CreateUserAndProfileCommand command,
        IWriteDbContext writeDbContext,
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

            Domain.Entity.User user = new(
                     Guid.NewGuid(),
                     DateTime.UtcNow,
                     DateTime.UtcNow,
                     command.Name,
                      passwordHash,
                     command.Email
            );


            Domain.Entity.Perfil perfil = new(
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
