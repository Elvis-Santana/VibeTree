using Microsoft.EntityFrameworkCore;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;

namespace VibeTree.Application.Perfil.Get.GetBySlug;

public partial class GetPerfilBySlugHandler (IReadDbContext appDbContext) : IHandler<GetPerfilBySlugQuery, PerfilResponse>
{
    public async Task<Result<PerfilResponse>> HandleAsync(GetPerfilBySlugQuery command)
    {
        var result = await appDbContext
            .perfils
            .FirstOrDefaultAsync(p => p.Slug.Equals(command.slug));

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
