using Microsoft.EntityFrameworkCore;
using UsuariosApi.DTOs;
using UsuariosApi.Entities;
using UsuariosApi.Services;

namespace UsuariosApi.Data;

/// <summary>
/// Cria o primeiro Administrador quando a base está vazia (bootstrap),
/// já que todos os endpoints de /api/usuarios exigem esse perfil.
/// </summary>
public static class SeedAdministrador
{
    public static async Task ExecutarAsync(IServiceProvider sp, IConfiguration config)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        if (await db.Usuarios.AnyAsync()) return;

        var email = config["Seed:Email"];
        var senha = config["Seed:Senha"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha)) return;

        var service = sp.GetRequiredService<IUsuarioService>();
        await service.CriarAsync(new CriarUsuarioRequest
        {
            Nome = config["Seed:Nome"] ?? "Administrador",
            Email = email,
            Senha = senha,
            Perfil = Perfil.Administrador
        });
    }
}
