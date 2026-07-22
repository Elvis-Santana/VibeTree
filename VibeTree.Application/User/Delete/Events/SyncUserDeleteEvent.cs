using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Application.User.Delete.Events;

public record SyncUserDeleteEvent(Domain.Entity.User User);


