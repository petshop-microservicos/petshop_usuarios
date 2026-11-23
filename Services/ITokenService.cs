using UsuariosApi.Entities;

namespace UsuariosApi.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) Gerar(Usuario usuario);
}
