# UsuariosApi

Microsserviço de **Usuários** do sistema de gerenciamento de petshop. Responsável por cadastro de usuários, atribuição de perfil e autenticação, emitindo o token JWT consumido pelos demais serviços.

## Requisitos funcionais

| ID | Descrição | Endpoint(s) |
|---|---|---|
| RF01 | Gerenciar usuários (cadastrar, editar, inativar) | `POST /api/usuarios`, `PUT /api/usuarios/{id}`, `PATCH /api/usuarios/{id}/inativar` |
| RF02 | Atribuir perfil ao usuário | `PUT /api/usuarios/{id}/perfil` (e campo `perfil` no cadastro) |
| RF03 | Autenticar usuário e emitir token de acesso contendo o perfil | `POST /api/auth/login` |

## Stack

- C# / .NET 10, ASP.NET Core (Controllers)
- Entity Framework Core + SQLite
- Autenticação JWT Bearer (HS256)
- Hash de senha com `PasswordHasher<T>` (Microsoft.Extensions.Identity.Core)
- OpenAPI (`/openapi/v1.json`, apenas em Development)

## Estrutura

```
UsuariosApi/
├── Controllers/     # UsuariosController, AuthController
├── Services/        # UsuarioService, AuthService, TokenService (+ interfaces)
├── DTOs/            # Requests/Responses
├── Entities/        # Usuario, Perfil
├── Data/            # AppDbContext, SeedAdministrador
├── Exceptions/      # Exceções de domínio + GlobalExceptionHandler
├── Configuration/   # JwtOptions
├── Program.cs
├── appsettings.json
└── usuarios.http    # requisições de exemplo (VS Code REST Client)
```

Os Services acessam o `AppDbContext` diretamente, e todas as dependências são registradas via DI nativa do ASP.NET Core.

## Modelo de dados

Tabela `usuarios`:

| Campo | Tipo | Observações |
|---|---|---|
| `Id` | Guid | PK |
| `Nome` | string(120) | obrigatório |
| `Email` | string(200) | obrigatório, **único**, armazenado em minúsculas |
| `SenhaHash` | string | hash PBKDF2 com salt; a senha nunca é armazenada nem retornada |
| `Perfil` | string(20) | `Administrador`, `Atendente` ou `Funcionario` (salvo como texto) |
| `Ativo` | bool | `false` = inativado (exclusão lógica) |
| `CriadoEm` / `AtualizadoEm` | DateTime (UTC) | |

## Perfis

| Perfil | Acesso a `/api/usuarios` |
|---|---|
| Administrador | Total |
| Atendente | Nenhum |
| Funcionario | Nenhum |

As permissões dos demais módulos (agendamentos, estoque, financeiro etc.) são aplicadas em cada serviço, a partir da claim `role` do token.

## Configuração

`appsettings.json`:

```json
{
  "ConnectionStrings": { "Default": "Data Source=usuarios.db" },
  "Jwt": {
    "Issuer": "petshop-usuarios",
    "Audience": "petshop-api",
    "Key": "<mínimo 32 bytes>",
    "ExpiracaoMinutos": 60
  },
  "Seed": {
    "Nome": "Administrador",
    "Email": "admin@petshop.local",
    "Senha": "Admin@12345"
  }
}
```

> **Atenção:** a `Jwt:Key` e a seção `Seed` do arquivo são apenas para desenvolvimento. Fora dele, defina por variável de ambiente (ex.: `Jwt__Key`, `Seed__Senha`). A aplicação não sobe se a chave tiver menos de 32 bytes.

**Seed:** na inicialização, se a tabela estiver vazia, é criado um Administrador com os dados de `Seed`. Isso é necessário porque todos os endpoints de `/api/usuarios` exigem esse perfil.

## Como executar

Pré-requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet tool install --global dotnet-ef     # uma vez, se necessário
dotnet ef migrations add Inicial           # uma vez, antes da primeira execução
dotnet run
```

As migrations são aplicadas automaticamente na subida (`Database.MigrateAsync()`), por isso a migration `Inicial` precisa existir. O banco `usuarios.db` é criado no diretório de execução.

## Autenticação (RF03)

`POST /api/auth/login`

```json
{ "email": "admin@petshop.local", "senha": "Admin@12345" }
```

Resposta `200`:

```json
{
  "accessToken": "eyJhbGciOi...",
  "tokenType": "Bearer",
  "expiraEm": "2026-11-20T18:00:00Z",
  "usuario": {
    "id": "…",
    "nome": "Administrador",
    "email": "admin@petshop.local",
    "perfil": "Administrador",
    "ativo": true,
    "criadoEm": "…",
    "atualizadoEm": null
  }
}
```

Use o token no header `Authorization: Bearer <accessToken>`.

### Claims do token

| Claim | Valor |
|---|---|
| `sub` | Id do usuário |
| `email` | E-mail |
| `name` | Nome |
| `role` | Perfil (`Administrador`, `Atendente` ou `Funcionario`) |
| `iss`, `aud`, `exp` | Emissor, audiência e expiração |

## Referência da API

### Usuários (perfil Administrador)

| Método | Rota | Descrição | Sucesso |
|---|---|---|---|
| POST | `/api/usuarios` | Cadastra usuário (`nome`, `email`, `senha`, `perfil`) | 201 |
| GET | `/api/usuarios?ativo=&perfil=` | Lista, com filtros opcionais | 200 |
| GET | `/api/usuarios/{id}` | Obtém por id | 200 |
| PUT | `/api/usuarios/{id}` | Edita `nome` e `email` | 200 |
| PATCH | `/api/usuarios/{id}/inativar` | Inativa o usuário | 200 |
| PATCH | `/api/usuarios/{id}/reativar` | Reativa o usuário | 200 |
| PUT | `/api/usuarios/{id}/perfil` | Atribui/troca o perfil (`{ "perfil": "Atendente" }`) | 200 |

### Autenticação

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/api/auth/login` | Anônimo | Autentica e emite o JWT |
| GET | `/api/auth/me` | Autenticado | Retorna as claims do token atual |

### Validações

- `nome`: 2 a 120 caracteres
- `email`: formato válido, até 200 caracteres, único
- `senha`: 8 a 100 caracteres
- `perfil`: obrigatório; aceita apenas os nomes (strings) dos perfis

## Regras de negócio

- E-mail é único (comparação sem diferenciar maiúsculas/minúsculas).
- Inativar é **exclusão lógica**: o registro permanece e o usuário não consegue autenticar.
- Não é possível **inativar nem alterar o perfil do único Administrador ativo**, para o sistema nunca ficar sem administrador.
- Credenciais inválidas e e-mail inexistente retornam a mesma resposta (`401`), sem revelar se o e-mail existe.
- Usuário inativo com senha correta recebe `403`.

## Tratamento de erros

Erros seguem o formato `ProblemDetails` (RFC 7807):

| Status | Situação |
|---|---|
| 400 | Falha de validação do payload |
| 401 | Credenciais inválidas ou token ausente/expirado |
| 403 | Usuário inativo ou perfil sem permissão |
| 404 | Usuário não encontrado |
| 409 | E-mail já cadastrado |
| 422 | Regra de negócio violada (ex.: último Administrador ativo) |
| 500 | Erro interno |

## Integração com os demais serviços

Cada serviço valida o JWT localmente, sem chamar o serviço de Usuários. Para isso, deve usar o mesmo `Issuer`, `Audience` e `Key`:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,           ValidIssuer = jwt.Issuer,
            ValidateAudience = true,         ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
```

Depois é só proteger os endpoints, por exemplo `[Authorize(Roles = "Administrador,Atendente")]`.

## Limitações conhecidas

- Chave simétrica compartilhada entre serviços: quem valida o token também poderia assiná-lo. Em produção, o ideal é RS256/ES256 com chave pública distribuída (JWKS).
- Sem refresh token, troca/recuperação de senha, bloqueio por tentativas de login ou revogação de token. Um usuário inativado continua com token válido até ele expirar (`ExpiracaoMinutos`).
- Não há testes automatizados nem Dockerfile/CI neste repositório ainda.
