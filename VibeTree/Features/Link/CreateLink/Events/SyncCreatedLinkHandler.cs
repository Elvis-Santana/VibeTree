using VibeTree.Features.User.CreateUser.Events;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.Link.CreateLink.Events;

public static class SyncCreatedLinkHandler
{
    public static async Task Handle(SyncCreatedLinkEvent @event, ReadDbContext readDbContext)
    {
        await readDbContext.Links.AddAsync(@event.Link);
        await readDbContext.SaveChangesAsync();
    }
}
