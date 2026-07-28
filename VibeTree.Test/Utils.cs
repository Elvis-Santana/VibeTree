using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Tracking;

namespace VibeTree.Test;

public static class Utils
{
    public static async Task<HttpResponseMessage> Cast(IHost host,Task<HttpResponseMessage> task)
    {
        HttpResponseMessage response = default!;

        Func<IMessageContext, Task> action = async _ => response = await task;

        await host.TrackActivity().Timeout(TimeSpan.FromSeconds(10)).ExecuteAndWaitAsync(action);

        return response;
    }
}