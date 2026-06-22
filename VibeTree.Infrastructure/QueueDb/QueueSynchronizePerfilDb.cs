using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using VibeTree.Domain.Entity;

namespace VibeTree.Infrastructure.Query;

public class QueueSynchronizePerfilDb : IQueueSynchronizeDb<SyncData<Perfil>>
{

    private readonly Channel<SyncData<Perfil>> _channel  = Channel.CreateBounded<SyncData<Perfil>>(new BoundedChannelOptions(1_000)
    {
        SingleWriter = true,
        FullMode = BoundedChannelFullMode.Wait
    });

    public bool HasPending => _channel.Reader.TryPeek(out _);

    public ValueTask AddJobAsync(SyncData<Perfil> jobSynchronize, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(jobSynchronize, cancellationToken);



    public  IAsyncEnumerable<SyncData<Perfil>> ReadAllAsync(CancellationToken cancellationToken)
        =>  _channel.Reader.ReadAllAsync(cancellationToken);
    
}
