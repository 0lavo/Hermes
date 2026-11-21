# Permissões no servidor (US15)

Todos os endpoints, exceto `register`, `login` e `google`, levam `[Authorize]`. Dentro de um controller, `User.GetUserId()` devolve o id do utilizador do token (`null` se não houver).

Para decidir se o utilizador pode agir numa sala, usar o `IRoomAccessService` (`backend/Hermes.Api/Auth/RoomAccessService.cs`), nunca confiar só na interface:

| Método | Quando usar | Resultados possíveis |
|---|---|---|
| `CheckOwnerAsync(código, userId)` | iniciar/encerrar sessão, marcar dúvida como respondida | `Allowed` (traz a sala), `RoomNotFound`, `NotOwner` |
| `CheckParticipantAsync(código, userId)` | submeter dúvida, votar | `Allowed` (traz a sessão ativa e o participante), `RoomNotFound`, `NoActiveSession`, `NotParticipant` |

Resposta HTTP sugerida: `RoomNotFound` → 404, `NotOwner` e `NotParticipant` → 403, `NoActiveSession` → 409. O dono nunca é participante. Os códigos de sala têm 6 dígitos (ex.: `482913`).

Exemplo num endpoint do dono:

~~~csharp
var access = await _access.CheckOwnerAsync(code, User.GetUserId()!.Value);

if (access.Result == AccessResult.RoomNotFound) return NotFound(new ErrorResponse("Sala não encontrada."));
if (access.Result == AccessResult.NotOwner)
    return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse("Só o dono da sala pode fazer isto."));

var room = access.Room!;
~~~

Também existe `GET /api/auth/me`, que devolve o utilizador do token (útil para o frontend recuperar a sessão depois de recarregar a página).
