using VibeTree.Application.Result;

namespace VibeTree.Application.Interfaces;

public interface IHandler<in TRequest, TResponse>  where TRequest : IRequest<TResponse>
{
     Task<Result<TResponse>> HandleAsync(TRequest command);
}
public interface IHandler <TResponse>
{
    Task<Result<TResponse>> HandleAsync( );


}
