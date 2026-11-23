using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UsuariosApi.Configuration;
using UsuariosApi.Entities;

namespace UsuariosApi.Services;

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _jwt = options.Value;

    // RF03 - token de acesso contendo o perfil (claim "role")
    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_jwt.ExpiracaoMinutos);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", usuario.Id.ToString()),
                new Claim("email", usuario.Email),
                new Claim("name", usuario.Nome),
                new Claim("role", usuario.Perfil.ToString())
            }),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = expiraEm,
            SigningCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiraEm);
    }
}
