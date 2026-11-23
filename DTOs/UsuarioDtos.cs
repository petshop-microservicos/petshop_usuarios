using System.ComponentModel.DataAnnotations;
using UsuariosApi.Entities;

namespace UsuariosApi.DTOs;

public class CriarUsuarioRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Senha { get; set; } = string.Empty;

    // Nullable + Required: evita que a ausência do campo vire o valor 0 (Administrador).
    [Required, EnumDataType(typeof(Perfil))]
    public Perfil? Perfil { get; set; }
}

public class AtualizarUsuarioRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;
}

public class AtribuirPerfilRequest
{
    [Required, EnumDataType(typeof(Perfil))]
    public Perfil? Perfil { get; set; }
}

public record UsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    Perfil Perfil,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm)
{
    public static UsuarioResponse De(Usuario u) =>
        new(u.Id, u.Nome, u.Email, u.Perfil, u.Ativo, u.CriadoEm, u.AtualizadoEm);
}
