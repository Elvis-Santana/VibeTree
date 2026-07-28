using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.Perfil.Get.GetById;

public partial class GetPerfilByIdHandler(ReadDbContext appDbContext) 
{

    public async Task<Result<PerfilResponse>> Handle(GetPerfilByIdQuery command)
    {

       var result = await appDbContext
            .perfils
            .FindAsync(Guid.Parse(command.id));

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
