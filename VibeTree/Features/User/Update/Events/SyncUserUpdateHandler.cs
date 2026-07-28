using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.User.Update.Events;

public static class SyncUserUpdateHandler
{
    public static async Task Handle(SyncUserUpdateEvent @event, ReadDbContext readDbContext)
    {
        readDbContext.Users.Update(@event.User);
        await readDbContext.SaveChangesAsync();
    }
}
