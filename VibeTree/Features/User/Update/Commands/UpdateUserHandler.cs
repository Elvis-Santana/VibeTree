using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeTree.Features.User.Update.Events;
using VibeTree.Shared.Auth;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Features.User.Update.Commands;

public class UpdateUserHandler (
        WriteDbContext writeDbContext,
        ITokenService tokenService,
        IValidator<UpdateUserCommand> validator
    ) 
{

    public async Task<(Result<Userlogin>, SyncUserUpdateEvent?)> Handle(UpdateUserCommand command)
    {
        ValidationResult validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return (validationResult.Errors.Select(a => new Error(a.ErrorMessage)).ToList(),null);

        Shared.Entity.User? user = await writeDbContext.Users.Include(a =>a.Perfil).FirstOrDefaultAsync(a => a.Id.Equals(Guid.Parse(command.Id)));
       
        if (user is null)
            return (Error.NotFound,null);

        user.SetName(command?.Name);
        user.SetEmail(command?.Email);
        user.setPasswordHash(command?.Password);

        await writeDbContext.SaveChangesAsync();

        string? tokem = !string.Equals(command?.Password,string.Empty)
                ? (await tokenService.CriarToken(user, user.Perfil)).token 
                : string.Empty;

        var @event = new SyncUserUpdateEvent(user);
        return (new Userlogin(user.Id, user.Name, user.Email, tokem),@event);
    }
}
