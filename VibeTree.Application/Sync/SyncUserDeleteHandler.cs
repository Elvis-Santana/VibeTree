using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.User.Create.Events;
using VibeTree.Application.User.Delete.Events;

namespace VibeTree.Application.Sync;

public static class SyncUserDeleteHandler
{
    public static async Task Handle(SyncUserDeleteEvent @event, IReadDbContext readDbContext)
    {
        readDbContext.Users.Remove(@event.User);
        await readDbContext.SaveChangesAsync();
    }
}
