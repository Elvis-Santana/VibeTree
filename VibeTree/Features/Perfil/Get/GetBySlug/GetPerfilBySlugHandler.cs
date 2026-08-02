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
           .AsNoTracking()
           .Where(p => p.Slug == command.slug)
           .Select(p => new PerfilResponse(
               p.Id,
               p.Descricao,
               p.ImagemUrl,
               p.Slug,
               p.IdUser
           ))
           .FirstOrDefaultAsync();

        return result is null
          ? new Error("perfil não encontrado")
          : result;
    }

  
}
