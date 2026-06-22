using System.Threading.Channels;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;

namespace VibeTree.Infrastructure.Query;

public class QueueSynchronizeUserDb : IQueueSynchronizeDb<SyncData<Domain.Entity.User>>
{

    private readonly Channel<SyncData<Domain.Entity.User>> _channel = Channel.CreateBounded<SyncData<Domain.Entity.User>>(
        new BoundedChannelOptions(1_000){
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        }
    );

    public bool HasPending => _channel.Reader.TryPeek(out _);

    public async ValueTask AddJobAsync(SyncData<Domain.Entity.User> jobSynchronize, CancellationToken cancellationToken)
       => await _channel.Writer.WriteAsync(jobSynchronize, cancellationToken);
    

    public  IAsyncEnumerable<SyncData<Domain.Entity.User>> ReadAllAsync(CancellationToken cancellationToken)
        =>  _channel.Reader.ReadAllAsync(cancellationToken);
    
}
