using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Application.Perfil.Create;

public class CreatePerfilHandler(
    AppDbContext appDbContext,
   IValidator<CreatePerfilCommand> validator
    IQueryJobSynchronize<SyncData<Domain.Entity.Perfil>> queryJobSynchronize

    ) : IHandler<CreatePerfilCommand, PerfilResponse>
{


    public async Task<Result<PerfilResponse>> HandleAsync(CreatePerfilCommand command)
    {
        var validationResult = await validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return validationResult
                .Errors
                .Select(e => new Error(e.ErrorMessage))
                .ToList();

        Domain.Entity.Perfil perfil = new Domain.Entity.Perfil(
               Guid.NewGuid(),
               DateTime.Now,
               DateTime.Now,
               command.Cor,
               command.Descricao,
               command.ImagemUrl,
               command.Slug,
               Guid.Parse(command.IdUser)
        );

        var user = await appDbContext
             .Users
             .FirstOrDefaultAsync((user) => user.Id.Equals(Guid.Parse(command.IdUser)));

        if (user is null)
            return new Error("não existe usuario com este idUser");
        


        await appDbContext.perfils.AddAsync(perfil);
        await appDbContext.SaveChangesAsync();


        return new PerfilResponse(perfil.Id, perfil.Descricao, perfil.ImagemUrl, perfil.Slug, perfil.IdUser);

    }
}
