using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeTree.Application.Auth;
using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User.Create.Events;
using Wolverine;
using Wolverine.Attributes;

namespace VibeTree.Application.User.Create.Commands;

public static class CreateUserAndProfileHandler {

    public static async Task<Result<Userlogin>> Handle( 
        CreateUserAndProfileCommand command, 
        IWriteDbContext writeDbContext,
        ITokenService tokenService,
        IValidator<CreateUserAndProfileCommand> validator,
        IMessageBus bus
    )
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var err = validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();
            return err;
        }

        bool existe = await writeDbContext
            .Users
            .AnyAsync(u=> u.Email.ToLower().Trim().Equals(command.Email.ToLower().Trim()));

        if (existe)
            return Error.ExisteUser;

            Domain.Entity.User user = new(
                Guid.NewGuid(),
                DateTime.UtcNow,
                DateTime.UtcNow,
                command.Name,
                BCrypt.Net.BCrypt.HashPassword(command.Password),
                command.Email
            );

            await writeDbContext.Users.AddAsync(user);

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

            await writeDbContext.perfils.AddAsync(perfil);

            await  writeDbContext.SaveChangesAsync();

            await bus.PublishAsync(new SyncUserPerfilCreatedEvent(user, perfil));

            Token token = await tokenService.CriarToken(user, perfil);

            return new Userlogin(user.Id, user.Name, user.Email, token.token);
    }

}
