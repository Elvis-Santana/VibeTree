using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeTree.Application.Sync;

public record class SyncData<T>(T Item, SyncOperation Operation);


