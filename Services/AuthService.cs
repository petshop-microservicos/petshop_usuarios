using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UsuariosApi.Data;
using UsuariosApi.DTOs;
using UsuariosApi.Entities;
using UsuariosApi.Exceptions;

namespace UsuariosApi.Services;

public class AuthService(AppDbContext db, IPasswordHasher<Usuario> hasher, ITokenService tokens) : IAuthService
{
    // RF03 - autenticar e emitir token
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct)
                      ?? throw new CredenciaisInvalidasException();

        var resultado = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (resultado == PasswordVerificationResult.Failed)
            throw new CredenciaisInvalidasException();

        if (!usuario.Ativo)
            throw new UsuarioInativoException();

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.SenhaHash = hasher.HashPassword(usuario, request.Senha);
            await db.SaveChangesAsync(ct);
        }

        var (token, expiraEm) = tokens.Gerar(usuario);
        return new LoginResponse(token, "Bearer", expiraEm, UsuarioResponse.De(usuario));
    }
}
