using UsuariosApi.DTOs;
using UsuariosApi.Entities;

namespace UsuariosApi.Services;

public interface IUsuarioService
{
    Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<UsuarioResponse>> ListarAsync(bool? ativo, Perfil? perfil, CancellationToken ct = default);
    Task<UsuarioResponse> ObterAsync(Guid id, CancellationToken ct = default);
    Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request, CancellationToken ct = default);
    Task<UsuarioResponse> InativarAsync(Guid id, CancellationToken ct = default);
    Task<UsuarioResponse> ReativarAsync(Guid id, CancellationToken ct = default);
    Task<UsuarioResponse> AtribuirPerfilAsync(Guid id, Perfil perfil, CancellationToken ct = default);
}
