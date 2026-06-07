using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VibeTree.Application.Interfaces.IQueryJob;

public interface IQueryJobSynchronize<T>
{


    public ValueTask AddJobAsync(T jobSynchronize, CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken =default);

    public bool HasPending { get; }

}
