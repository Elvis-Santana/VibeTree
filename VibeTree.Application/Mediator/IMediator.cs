using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;

namespace VibeTree.Application.Mediator;

public interface IMediator
{
    public Task<Result<IResponse>> SendAync<IResponse>(IResquest<IResponse> resquest);
}
