namespace VibeTree.Features.Link.CreateLink.Commands;

public record CreateLinkCommand(string LinkUrl,string Descricao, string IdPerfil, decimal Order,bool Ativo);


