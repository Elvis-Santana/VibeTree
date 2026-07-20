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

public class QueueSynchronize<T> : IQueueSynchronizeDb<SyncData<T>>
{

    private readonly Channel<SyncData<T>> _channel  = Channel.CreateBounded<SyncData<T>>(new BoundedChannelOptions(1_000)
    {
        SingleWriter = true,
        FullMode = BoundedChannelFullMode.Wait
    });

    public bool HasPending => _channel.Reader.TryPeek(out _);

    public ValueTask AddJobAsync(SyncData<T> jobSynchronize, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(jobSynchronize, cancellationToken);



    public  IAsyncEnumerable<SyncData<T>> ReadAllAsync(CancellationToken cancellationToken)
        =>  _channel.Reader.ReadAllAsync(cancellationToken);
    
}
