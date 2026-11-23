using UsuariosApi.DTOs;

namespace UsuariosApi.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
