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
    public static async Task<Result<bool>> Handle(
         this DeleteUserCommand command,
         IWriteDbContext writeDbContext,
         IValidator<DeleteUserCommand> validator,
         IMessageBus bus
    )
    {
       var validationResult = await validator.ValidateAsync( command );

        if(!validationResult.IsValid)
            return validationResult.Errors.Select(a => new Error(a.ErrorMessage)).ToList();

        if (!Guid.TryParse(command.Id, out var id))
            return Error.IdValid;

       
        Domain.Entity.User? user =  await writeDbContext.Users.FindAsync(id);

        if (user is null)
            return Error.NotFound;

        try
        {
            writeDbContext.Users.Remove(user);
            await writeDbContext.SaveChangesAsync();
            await bus.PublishAsync(new SyncUserDeleteEvent(user));
            return true;

        }
        catch (DbUpdateException)
        {
            return new Error("Não foi possível excluir o usuário devido a restrições no banco de dados.");
        }

    }
}
