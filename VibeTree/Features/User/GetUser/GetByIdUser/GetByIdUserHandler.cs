using VibeTree.Shared.Common;
using VibeTree.Shared.DbAppContext;

namespace VibeTree.Features.User.GetUser.GetByIdUser;

public class GetByIdUserHandler (ReadDbContext readDbContext)
{
    public async Task<Result<UserResponse>> Handle(GetByIdUserQuery command)
    {
        if (!Guid.TryParse(command.id, out var idGuid))
           return Error.IdValid;

        Shared.Entity.User? user =  await readDbContext.Users.FindAsync(idGuid);

        if (user is null)
            return Error.UserNotFound;
        
        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
