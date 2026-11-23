using System.ComponentModel.DataAnnotations;

namespace UsuariosApi.DTOs;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

public record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiraEm,
    UsuarioResponse Usuario);
