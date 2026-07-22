using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Common;
using VibeTree.Application.Sync;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VibeTree.Application.User.Update;

public class UpdateUserHandler (
        IWriteDbContext writeDbContext,
        ITokenService tokenService,
        IValidator<UpdateUserCommand> validator,
        IQueueSynchronizeDb<SyncData<Domain.Entity.User>> queryJobSynchronize
    ) : IHandler<UpdateUserCommand, Userlogin>
{

    public async Task<Result<Userlogin>> HandleAsync(UpdateUserCommand command)
    {
        ValidationResult validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return validationResult.Errors.Select(a => new Error(a.ErrorMessage)).ToList();

        Domain.Entity.User? user = await writeDbContext.Users.Include(a =>a.Perfil).FirstOrDefaultAsync(a => a.Id.Equals(Guid.Parse(command.Id)));
       
        if (user is null)
            return Error.NotFound;

        user.SetName(command?.Name);
        user.SetEmail(command?.Email);
        user.setPasswordHash(command?.Password);

        await writeDbContext.SaveChangesAsync();

        string? tokem = !string.Equals(command?.Password,string.Empty)
                ? (await tokenService.CriarToken(user, user.Perfil)).token 
                : string.Empty;

        await queryJobSynchronize.AddJobAsync(new SyncData<Domain.Entity.User>(user, SyncOperation.Update));
        return new Userlogin(user.Id, user.Name, user.Email, tokem);
    }
}
