namespace UsuariosApi.Exceptions;

public abstract class ExcecaoDeDominio(string mensagem, int statusCode, string titulo) : Exception(mensagem)
{
    public int StatusCode { get; } = statusCode;
    public string Titulo { get; } = titulo;
}

public class NaoEncontradoException(string mensagem)
    : ExcecaoDeDominio(mensagem, StatusCodes.Status404NotFound, "Recurso não encontrado");

public class ConflitoException(string mensagem)
    : ExcecaoDeDominio(mensagem, StatusCodes.Status409Conflict, "Conflito");

public class RegraNegocioException(string mensagem)
    : ExcecaoDeDominio(mensagem, StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada");

public class CredenciaisInvalidasException()
    : ExcecaoDeDominio("E-mail ou senha inválidos.", StatusCodes.Status401Unauthorized, "Não autenticado");

public class UsuarioInativoException()
    : ExcecaoDeDominio("Usuário inativo.", StatusCodes.Status403Forbidden, "Acesso negado");
