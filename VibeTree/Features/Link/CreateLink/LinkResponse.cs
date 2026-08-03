namespace VibeTree.Features.Link.CreateLink;

public record LinkResponse(Guid Id,string LinkUrl, string Descricao, Guid IdPerfil, decimal Order, bool Ativo);


