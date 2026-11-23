using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsuariosApi.DTOs;
using UsuariosApi.Entities;
using UsuariosApi.Services;

namespace UsuariosApi.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = nameof(Perfil.Administrador))]
public class UsuariosController(IUsuarioService service) : ControllerBase
{
    /// <summary>RF01 - Cadastra usuário já com perfil (RF02).</summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioResponse>> Criar(CriarUsuarioRequest request, CancellationToken ct)
    {
        var criado = await service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponse>>> Listar(
        [FromQuery] bool? ativo, [FromQuery] Perfil? perfil, CancellationToken ct)
        => Ok(await service.ListarAsync(ativo, perfil, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioResponse>> Obter(Guid id, CancellationToken ct)
        => Ok(await service.ObterAsync(id, ct));

    /// <summary>RF01 - Edita nome e e-mail.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioResponse>> Atualizar(Guid id, AtualizarUsuarioRequest request, CancellationToken ct)
        => Ok(await service.AtualizarAsync(id, request, ct));

    /// <summary>RF01 - Inativa o usuário (exclusão lógica).</summary>
    [HttpPatch("{id:guid}/inativar")]
    public async Task<ActionResult<UsuarioResponse>> Inativar(Guid id, CancellationToken ct)
        => Ok(await service.InativarAsync(id, ct));

    [HttpPatch("{id:guid}/reativar")]
    public async Task<ActionResult<UsuarioResponse>> Reativar(Guid id, CancellationToken ct)
        => Ok(await service.ReativarAsync(id, ct));

    /// <summary>RF02 - Atribui (ou troca) o perfil do usuário.</summary>
    [HttpPut("{id:guid}/perfil")]
    public async Task<ActionResult<UsuarioResponse>> AtribuirPerfil(Guid id, AtribuirPerfilRequest request, CancellationToken ct)
        => Ok(await service.AtribuirPerfilAsync(id, request.Perfil!.Value, ct));
}
