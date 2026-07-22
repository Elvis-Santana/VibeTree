using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;

namespace VibeTree.Application.User.Delete;

public class DeleteUserHandler(
     IWriteDbContext writeDbContext,
     IValidator<DeleteUserCommand> validator,
     IQueueSynchronizeDb<SyncData<Domain.Entity.User>> queryJobSynchronize
    ) : IHandler<DeleteUserCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(DeleteUserCommand command)
    {
       var validationResult = await validator.ValidateAsync( command );

        if(!validationResult.IsValid)
            return validationResult.Errors.Select(a => new Error(a.ErrorMessage)).ToList();

         Domain.Entity.User? user =  await writeDbContext.Users.FindAsync(Guid.Parse(command.Id));

        if (user is null)
            return Error.NotFound;

        try
        {
            writeDbContext.Users.Remove(user);
            await writeDbContext.SaveChangesAsync();
            await queryJobSynchronize.AddJobAsync(new(user, SyncOperation.Update));
            return true;

        }
        catch (DbUpdateException)
        {
            return new Error("Não foi possível excluir o usuário devido a restrições no banco de dados.");
        }

    }
}
