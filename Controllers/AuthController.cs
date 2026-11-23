using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsuariosApi.DTOs;
using UsuariosApi.Services;

namespace UsuariosApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>RF03 - Autentica o usuário e emite o token JWT com o perfil.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
        => Ok(await auth.LoginAsync(request, ct));

    /// <summary>Retorna as claims do token atual (útil para validar o token).</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        Id = User.FindFirstValue("sub"),
        Nome = User.FindFirstValue("name"),
        Email = User.FindFirstValue("email"),
        Perfil = User.FindFirstValue("role")
    });
}
