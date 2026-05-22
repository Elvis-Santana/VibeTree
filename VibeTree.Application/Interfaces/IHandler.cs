using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Result;
using VibeTree.Application.User.Get;

namespace VibeTree.Application.Interfaces;

public interface IHandler<in TRequest, TResponse>  where TRequest : IRequest<TResponse>
{
     Task<Result<TResponse>> HandleAsync(TRequest command);
}
public interface IHandler <TResponse>
{
    Task<Result<TResponse>> HandleAsync( );


}
