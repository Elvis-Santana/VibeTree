using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;

namespace VibeTree.Application.Perfil.Get.GetById;

public record GetPerfilByIdQuery(string id) : IRequest<PerfilResponse>;


