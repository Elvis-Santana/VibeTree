using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;

namespace VibeTree.Infrastructure.Worker;

public abstract class WorkerSynchronizeBase<T>(
    IQueueSynchronizeDb<SyncData<T>> jobSynchronize, 
    IServiceProvider serviceProvider,
    ILogger logger 
) : BackgroundService
{
    private const int batchSize = 50;

 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<SyncData<T>>(batchSize);

        await foreach (var data in jobSynchronize.ReadAllAsync(stoppingToken))
        {
            batch.Add(data);
            if (batch.Count >= batchSize || !jobSynchronize.HasPending)
            {
                await Processing(batch, stoppingToken, serviceProvider);
                batch.Clear();
            }

        }

        if (batch.Count > 0)
            await Processing(batch, stoppingToken, serviceProvider);

    }
    protected abstract Task Processing(IEnumerable<SyncData<T>> data, CancellationToken cancellationToken, IServiceProvider serviceProvider);

   
}
