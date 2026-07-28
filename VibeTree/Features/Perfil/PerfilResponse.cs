using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Features.Perfil;

public record PerfilResponse(Guid Id, string Descricao, string ImagemUrl, string Slug, Guid IdUser);


