namespace VibeTree.Application.Auth;

public interface ITokenService
{
    Task<Token> CriarToken(Domain.Entity.User user ,Domain.Entity.Perfil perfil);
}
