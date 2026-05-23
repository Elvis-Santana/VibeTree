using VibeTree.Application.Interfaces;

namespace VibeTree.Application.Perfil.Get.GetBySlug;

public record GetPerfilBySlugQuery(string slug) : IRequest<PerfilResponse>;


