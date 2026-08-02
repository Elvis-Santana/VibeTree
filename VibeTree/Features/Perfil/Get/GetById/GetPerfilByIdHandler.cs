using Microsoft.EntityFrameworkCore;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.Perfil.Get.GetById;

public partial class GetPerfilByIdHandler(ReadDbContext appDbContext) 
{

    public async Task<Result<PerfilResponse>> Handle(GetPerfilByIdQuery command)
    {

        if (!Guid.TryParse(command.id, out var idGuid))
            return Error.IdValid;

        var result = await appDbContext
             .perfils
             .AsNoTracking()
             .Where(p => p.Id == idGuid)
             .Select(p =>
                 new PerfilResponse(
                 p!.Id,
                 p.Descricao,
                 p.ImagemUrl,
                 p.Slug,
                 p.IdUser)
             ).FirstOrDefaultAsync();


        return result is null
          ? new Error("perfil não encontrado")
          : result;

    }
}
