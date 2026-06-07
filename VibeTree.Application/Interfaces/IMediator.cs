using VibeTree.Application.Result;

namespace VibeTree.Application.Interfaces;

public interface IMediator
{
    public Task<Result<IResponse>> SendAsync<IResponse>(IRequest<IResponse> resquest);
}
