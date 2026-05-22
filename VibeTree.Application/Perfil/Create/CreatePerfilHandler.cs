using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Infrastructure.AppDbContext;

namespace VibeTree.Application.Perfil.Create;

public class CreatePerfilHandler(AppDbContext appDbContext) : IHandler<CreatePerfilCommand, PerfilResponse>
{
    private readonly AppDbContext _appDbContext = appDbContext;


    public async Task<Result<PerfilResponse>> HandleAsync(CreatePerfilCommand command)
    {

        Domain.Entity.Perfil perfil = new Domain.Entity.Perfil(
               Guid.NewGuid(),
               DateTime.Now,
               DateTime.Now,
               command.Cor,
               command.Descricao,
               command.ImagemUrl,
               command.Slug,
               command.IdUser
        );

        await _appDbContext.perfils.AddAsync(perfil);
        await _appDbContext.SaveChangesAsync();


        return new PerfilResponse(perfil.Id, perfil.Descricao, perfil.ImagemUrl, perfil.Slug, perfil.IdUser);


    }
}
