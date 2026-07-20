using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Application.Result;
using VibeTree.User.CreateUser;

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

        writeDbContext.Users.Remove(user);
        bool isSave = await writeDbContext.SaveChangesAsync() >0 ;
        await queryJobSynchronize.AddJobAsync(new (user,SyncOperation.Update));


        return isSave;
    }
}
