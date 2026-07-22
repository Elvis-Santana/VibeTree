using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Domain.Entity;

namespace VibeTree.Application.User.Create.Events;

public  record SyncUserPerfilCreatedEvent(Domain.Entity.User User , Domain.Entity.Perfil Perfil);


