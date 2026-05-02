using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.User.Get;

namespace VibeTree.Application.Interfaces;

public interface IHandler<TResquest, TResponse>  where TResquest : IResquest<TResponse>
{
     Task<TResponse> HandleAsync(TResquest command);
}
public interface IHandler <TResponse>
{


    Task<TResponse> HandleAsync( );


}
