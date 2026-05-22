using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;
using VibeTree.Application.User;

namespace VibeTree.Application.Perfil.Create;

public record CreatePerfilCommand(string Cor, string Descricao, string ImagemUrl, string Slug, Guid IdUser): IRequest<PerfilResponse>;


