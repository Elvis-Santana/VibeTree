using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User.Create.Events;

namespace VibeTree.Application.Sync;

public static class SyncUserPerfilHandler
{

    public static async Task Handle(SyncUserPerfilCreatedEvent @event, IReadDbContext readDbContext)
    {
        await  readDbContext.Users.AddAsync(@event.User);
        await readDbContext.perfils.AddAsync(@event.Perfil);
        await readDbContext.SaveChangesAsync();
    }
}
