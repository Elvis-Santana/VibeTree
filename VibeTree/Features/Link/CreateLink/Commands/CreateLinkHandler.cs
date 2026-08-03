using FluentValidation;
using VibeTree.Features.Link.CreateLink.Events;
using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.Link.CreateLink.Commands;

public class CreateLinkHandler(WriteDbContext _writeDbContext, IValidator<CreateLinkCommand> _validator)
{
    public async Task<(Result<LinkResponse>, SyncCreatedLinkEvent?)> Handle(CreateLinkCommand createLinkCommand)
    {
        var validationResult = await  _validator.ValidateAsync(createLinkCommand);

        if (!validationResult.IsValid)
            return (validationResult.Errors.Select(e => new Error(e.ErrorMessage)).ToList(),null);
             
        

        if (!Guid.TryParse(createLinkCommand.IdPerfil,out Guid IdPerfil))
            return (Error.IdValid,null);
        

        Shared.Entity.Link link = new (
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            createLinkCommand.LinkUrl,
            createLinkCommand.Descricao,
            IdPerfil,
            createLinkCommand.Order,
            createLinkCommand.Ativo
         );

        await _writeDbContext.Links.AddAsync(link);
        await _writeDbContext.SaveChangesAsync();
        var @event = new SyncCreatedLinkEvent(link);


        return (
            new LinkResponse(link.Id, link.LinkUrl, link.Descricao, link.IdPerfil, link.Order, link.Ativo),
            @event
        );

    }
}
