using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;

namespace VibeTree.Application.Perfil.Get.GetById;

public partial class GetPerfilByIdHandler(IReadDbContext appDbContext) : IHandler<GetPerfilByIdQuery, PerfilResponse>
{

    public async Task<Result<PerfilResponse>> HandleAsync(GetPerfilByIdQuery command)
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
