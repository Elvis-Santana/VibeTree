using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Common;
using VibeTree.Domain;

namespace VibeTree.Application.User.Get;

public class GetAllUserHandler : IHandler<GetAllUserQuery, Userlogin>
{
    public GetAllUserHandler(IReadDbContext appDbContext)
    {
    }

    public Task<Result<Userlogin>> HandleAsync(GetAllUserQuery command)
    {
        throw new NotImplementedException();
    }
}
