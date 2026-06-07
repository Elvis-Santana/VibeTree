using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;

namespace VibeTree.Application.Perfil.Get.GetById;

public partial class GetPerfilByIdHandler(IReadDbContext appDbContext) : IHandler<GetPerfilByIdQuery, PerfilResponse>
{
    public async Task<Result<PerfilResponse>> HandleAsync(GetPerfilByIdQuery command)
    {

       var result = await appDbContext
            .perfils
            .FirstOrDefaultAsync(p => p.Id.Equals(Guid.Parse(command.id)));

        return result is null
          ? new Error("perfil não encontrado")
          : new PerfilResponse(
                result!.Id, 
                result.Descricao,
                result.ImagemUrl,
                result.Slug,
                result.IdUser
          );

    }
}
