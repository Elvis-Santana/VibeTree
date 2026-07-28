using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.User.CreateUser.Events;

public static class SyncUserPerfilHandler
{

    public static async Task Handle(SyncUserPerfilCreatedEvent @event, ReadDbContext readDbContext)
    {
        await  readDbContext.Users.AddAsync(@event.User);
        await readDbContext.perfils.AddAsync(@event.Perfil);
        await readDbContext.SaveChangesAsync();
    }
}
