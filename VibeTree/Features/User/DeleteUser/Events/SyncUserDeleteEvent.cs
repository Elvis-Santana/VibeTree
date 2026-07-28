using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Features.User.DeleteUser.Events;

public record SyncUserDeleteEvent(Shared.Entity.User User);


