using Microsoft.EntityFrameworkCore;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.Perfil.Get.GetBySlug;

public partial class GetPerfilBySlugHandler (ReadDbContext appDbContext) 
{
    public async Task<Result<PerfilResponse>> Handle(GetPerfilBySlugQuery command)
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
