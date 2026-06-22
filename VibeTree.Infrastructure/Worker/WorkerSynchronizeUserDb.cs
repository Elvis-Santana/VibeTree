using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Interfaces.IQueryJob;
using VibeTree.Application.Sync;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VibeTree.Infrastructure.Worker;

public class WorkerSynchronizeUserDb(
    IQueueSynchronizeDb<SyncData<Domain.Entity.User>>
    jobSynchronize, IServiceProvider serviceProvider,
        ILogger<WorkerSynchronizeUserDb> logger

    ) : WorkerSynchronizeBase<Domain.Entity.User>(jobSynchronize, serviceProvider, logger)
{


    protected async override Task Processing(IEnumerable<SyncData<Domain.Entity.User>> data, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var _service = scope.ServiceProvider.GetRequiredService<IReadDbContext>();

        try
        {
           var toInsert = data.Where(a => a.Operation.Equals(SyncOperation.Create)).Select(a => a.Item);
            if (toInsert.Any())
                await _service.Users.AddRangeAsync(toInsert, cancellationToken);


            await _service.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error processing batch: {ex.Message}");
            
        }
    }
}
