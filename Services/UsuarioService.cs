using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UsuariosApi.Data;
using UsuariosApi.DTOs;
using UsuariosApi.Entities;
using UsuariosApi.Exceptions;

namespace UsuariosApi.Services;

public class UsuarioService(AppDbContext db, IPasswordHasher<Usuario> hasher) : IUsuarioService
{
    // RF01 - cadastrar (já com perfil: RF02)
    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken ct = default)
    {
        var email = NormalizarEmail(request.Email);
        await GarantirEmailDisponivelAsync(email, null, ct);

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = email,
            Perfil = request.Perfil!.Value
        };
        usuario.SenhaHash = hasher.HashPassword(usuario, request.Senha);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
        return UsuarioResponse.De(usuario);
    }

    public async Task<IReadOnlyList<UsuarioResponse>> ListarAsync(bool? ativo, Perfil? perfil, CancellationToken ct = default)
    {
        var query = db.Usuarios.AsNoTracking();
        if (ativo.HasValue) query = query.Where(u => u.Ativo == ativo.Value);
        if (perfil.HasValue) query = query.Where(u => u.Perfil == perfil.Value);

        var usuarios = await query.OrderBy(u => u.Nome).ToListAsync(ct);
        return usuarios.Select(UsuarioResponse.De).ToList();
    }

    public async Task<UsuarioResponse> ObterAsync(Guid id, CancellationToken ct = default)
        => UsuarioResponse.De(await BuscarAsync(id, ct));

    // RF01 - editar
    public async Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request, CancellationToken ct = default)
    {
        var usuario = await BuscarAsync(id, ct);
        var email = NormalizarEmail(request.Email);
        await GarantirEmailDisponivelAsync(email, id, ct);

        usuario.Nome = request.Nome.Trim();
        usuario.Email = email;
        usuario.AtualizadoEm = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return UsuarioResponse.De(usuario);
    }

    // RF01 - inativar (exclusão lógica; o registro é mantido)
    public async Task<UsuarioResponse> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await BuscarAsync(id, ct);
        if (!usuario.Ativo) return UsuarioResponse.De(usuario);

        await GarantirQueNaoEhUltimoAdminAtivoAsync(usuario, "Não é possível inativar o único Administrador ativo.", ct);

        usuario.Ativo = false;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return UsuarioResponse.De(usuario);
    }

    public async Task<UsuarioResponse> ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await BuscarAsync(id, ct);
        if (usuario.Ativo) return UsuarioResponse.De(usuario);

        usuario.Ativo = true;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return UsuarioResponse.De(usuario);
    }

    // RF02 - atribuir perfil
    public async Task<UsuarioResponse> AtribuirPerfilAsync(Guid id, Perfil perfil, CancellationToken ct = default)
    {
        var usuario = await BuscarAsync(id, ct);
        if (usuario.Perfil == perfil) return UsuarioResponse.De(usuario);

        await GarantirQueNaoEhUltimoAdminAtivoAsync(usuario, "Não é possível alterar o perfil do único Administrador ativo.", ct);

        usuario.Perfil = perfil;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return UsuarioResponse.De(usuario);
    }

    // ---------- helpers ----------

    private async Task<Usuario> BuscarAsync(Guid id, CancellationToken ct)
        => await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct)
           ?? throw new NaoEncontradoException("Usuário não encontrado.");

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private async Task GarantirEmailDisponivelAsync(string email, Guid? ignorarId, CancellationToken ct)
    {
        var existe = await db.Usuarios.AnyAsync(u => u.Email == email && u.Id != ignorarId, ct);
        if (existe) throw new ConflitoException("Já existe um usuário com este e-mail.");
    }

    // Evita deixar o sistema sem nenhum Administrador ativo.
    private async Task GarantirQueNaoEhUltimoAdminAtivoAsync(Usuario usuario, string mensagem, CancellationToken ct)
    {
        if (usuario.Perfil != Perfil.Administrador || !usuario.Ativo) return;

        var outros = await db.Usuarios.CountAsync(
            u => u.Id != usuario.Id && u.Ativo && u.Perfil == Perfil.Administrador, ct);

        if (outros == 0) throw new RegraNegocioException(mensagem);
    }
}
