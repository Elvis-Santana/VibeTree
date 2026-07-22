using Microsoft.Extensions.DependencyInjection;
using VibeTree.Application.Common;
using VibeTree.Application.Interfaces;

namespace VibeTree.Infrastructure.Mediator;

public sealed class Mediator(IServiceProvider provider) : IMediator
{
    public async Task<Result<IResponse>> SendAsync<IResponse>(IRequest<IResponse> request) 
    {

        Type type = request.GetType();

        Type handlerType = typeof(IHandler<,>).MakeGenericType(type, typeof(IResponse));

        dynamic handler = provider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)request);
        
    }

  
}
