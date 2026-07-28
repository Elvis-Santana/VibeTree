using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.User.Delete.Events;

public static class SyncUserDeleteHandler
{
    public static async Task Handle(SyncUserDeleteEvent @event, ReadDbContext readDbContext)
    {
        readDbContext.Users.Remove(@event.User);
        await readDbContext.SaveChangesAsync();
    }
}
