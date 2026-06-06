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
using VibeTree.Application.QuerySync;
using VibeTree.Domain.Entity;

namespace VibeTree.Infrastructure.Worker;

public class WorkerSynchronizePerfilDb(
    IQueryJobSynchronize<SyncData<Perfil>> jobSynchronize,
    IServiceProvider serviceProvider,
            ILogger<WorkerSynchronizePerfilDb> logger
)
    : WorkerSynchronizeBase<Perfil>(jobSynchronize, serviceProvider, logger)
{


    protected async override Task Processing(IEnumerable<SyncData<Perfil>> data, CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var _service = scope.ServiceProvider.GetRequiredService<IReadDbContext>();

        try
        {
            var toInsert = data.Where(a => a.Operation.Equals(SyncOperation.Create)).Select(a => a.Item);
            if (toInsert.Any())
                await _service.perfils.AddRangeAsync(toInsert, cancellationToken);
            await _service.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error processing batch: {ex.Message}");
        }
    }
}


