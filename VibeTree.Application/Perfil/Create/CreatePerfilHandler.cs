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

    ) : IHandler<CreatePerfilCommand, PerfilResponse>
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IValidator<CreatePerfilCommand> _validator = validator;


    public async Task<Result<PerfilResponse>> HandleAsync(CreatePerfilCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList();

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

        var user = await _appDbContext
             .Users
             .Where((user) => user.Id.Equals(Guid.Parse(command.IdUser)))
             .FirstOrDefaultAsync();

        if (user is null)
        {
            return new Error("não existe usuario com este idUser");
        }


        await _appDbContext.perfils.AddAsync(perfil);
        await _appDbContext.SaveChangesAsync();


        return new PerfilResponse(perfil.Id, perfil.Descricao, perfil.ImagemUrl, perfil.Slug, perfil.IdUser);


    }
}
