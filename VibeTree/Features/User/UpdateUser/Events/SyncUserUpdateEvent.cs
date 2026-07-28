using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Features.User.UpdateUser.Events;

public record SyncUserUpdateEvent(Shared.Entity.User User);


