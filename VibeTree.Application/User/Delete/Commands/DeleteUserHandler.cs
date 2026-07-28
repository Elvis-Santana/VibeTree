using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Application.User.Delete.Events;
using Wolverine;

namespace VibeTree.Application.User.Delete.Commands;

public static class DeleteUserHandler
{
    public static async Task<(Result<bool>, SyncUserDeleteEvent?)> Handle(
         this DeleteUserCommand command,
         IWriteDbContext writeDbContext,
         IValidator<DeleteUserCommand> validator
    )
    {
       var validationResult = await validator.ValidateAsync( command );

        if(!validationResult.IsValid)
            return (validationResult.Errors.Select(a => new Error(a.ErrorMessage)).ToList(),null);

        if (!Guid.TryParse(command.Id, out var id))
            return (Error.IdValid,null);

       
        Domain.Entity.User? user =  await writeDbContext.Users.FindAsync(id);

        if (user is null)
            return (Error.UserNotFound,null);

       
            writeDbContext.Users.Remove(user);
            await writeDbContext.SaveChangesAsync();
        var @event = new SyncUserDeleteEvent(user);
            return (true, @event);

        
        

    }
}
