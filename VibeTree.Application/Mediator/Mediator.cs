using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Mediator;

namespace VibeTree.Application.Mediator;

public sealed class Mediator (IServiceProvider provider) : IMediator
{
    public async Task<Result.Result<IResponse>> SendAync<IResponse>(IResquest<IResponse> request)
    {

        
        Type type = request.GetType();

        var handlerType = typeof(IHandler<,>).MakeGenericType(type, typeof(IResponse));

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)request);
        
    }
}
