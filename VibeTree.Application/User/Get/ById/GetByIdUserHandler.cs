using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Application.Interfaces;
using VibeTree.Application.Result;

namespace VibeTree.Application.User.Get.ById;

public class GetByIdUserHandler (IReadDbContext readDbContext): IHandler<GetByIdUserQuery, UserResponse>
{
    public async Task<Result<UserResponse>> HandleAsync(GetByIdUserQuery command)
    {
        if (!Guid.TryParse(command.id, out var idGuid))
           return Error.IdValid;

        Domain.Entity.User? user =  await readDbContext.Users.FindAsync(idGuid);

        if (user is null)
            return Error.UserNotFound;
        
        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
