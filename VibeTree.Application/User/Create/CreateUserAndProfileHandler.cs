using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using VibeTree.Application.Auth;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Result;
using VibeTree.Application.Sync;
using VibeTree.Application.User;
using VibeTree.Domain.Entity;

namespace VibeTree.User.CreateUser;

public class CreateUserAndProfileHandler (
    IWriteDbContext appDbContext,
    ITokenService tokenService,
    IValidator<CreateUserAndProfileCommand> validator,
    IQueueSynchronizeDb<SyncData<Domain.Entity.User>> queryJobSynchronizeUser,
    IQueueSynchronizeDb<SyncData<Perfil>> queryJobSynchronizePerfil

    ) : IHandler<CreateUserAndProfileCommand, Userlogin>
{


    public async Task<Result<Userlogin>> HandleAsync(CreateUserAndProfileCommand command)
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();

       bool existe = await appDbContext
            .Users
            .AnyAsync(u=> u.Email.ToLower().Trim().Equals(command.Email.ToLower().Trim()));

        if (existe)
            return Error.ExisteUser;

        using var transaction = await appDbContext.Database.BeginTransactionAsync();

        try
        {
            Domain.Entity.User user = new(
               Guid.NewGuid(),
               DateTime.UtcNow,
               DateTime.UtcNow,
               command.Name,
               BCrypt.Net.BCrypt.HashPassword(command.Password),
               command.Email
            );

            await appDbContext.Users.AddAsync(user);
            await appDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            Perfil perfil = new(
                Guid.NewGuid(),
                DateTime.UtcNow,
                DateTime.UtcNow,
                string.Empty,
                string.Empty,
                string.Empty,
                command.Slug,
                user.Id
            );

            await appDbContext.perfils.AddAsync(perfil);
            await appDbContext.SaveChangesAsync();

            await queryJobSynchronizeUser.AddJobAsync(new SyncData<Domain.Entity.User>(user, SyncOperation.Create));
            await queryJobSynchronizePerfil.AddJobAsync(new SyncData<Perfil>(perfil, SyncOperation.Create));


            Token token = await tokenService.CriarToken(user, perfil);
            return new Userlogin(user.Id, user.Name, user.Email, token.token);
        }
        catch (Exception)
        {
           await  transaction.RollbackAsync();
           return Error.FalhaAoCadastrar;
        }
          
    }

}
