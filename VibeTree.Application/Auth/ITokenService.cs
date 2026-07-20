using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VibeTree.Domain.Entity;
using VibeTree.User.CreateUser;

namespace VibeTree.Application.Auth;

public interface ITokenService
{
    Task<Token> CriarToken(Domain.Entity.User user ,Domain.Entity.Perfil perfil);
}
