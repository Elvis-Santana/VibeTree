namespace VibeTree.Shared.Auth;

public interface ITokenService
{
    Task<Token> CriarToken(Entity.User user , Entity.Perfil perfil);
}
