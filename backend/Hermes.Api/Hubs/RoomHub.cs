using Hermes.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Hermes.Api.Hubs;

// Tempo real das salas (US14). O hub só envia avisos do servidor para os clientes; as alterações vão por REST.
// Um grupo por sala (o código): o dono e os participantes recebem os mesmos eventos,
// porque o anonimato já vem resolvido no payload (author = null nas dúvidas anónimas).
[Authorize]
public class RoomHub : Hub
{
    private readonly IRoomAccessService _access;

    public RoomHub(IRoomAccessService access)
    {
        _access = access;
    }

    // O cliente chama isto ao ligar e sempre que reconecta: os grupos perdem-se quando a ligação cai.
    // Só entram o dono da sala ou um participante da sessão ativa (RNF05, RNF14).
    public async Task JoinRoom(string code)
    {
        var userId = Context.User?.GetUserId() ?? throw new HubException("Sessão inválida.");

        var owner = await _access.CheckOwnerAsync(code, userId);
        if (owner.Result == AccessResult.RoomNotFound)
        {
            throw new HubException("Sala não encontrada.");
        }

        if (owner.Result != AccessResult.Allowed)
        {
            var participant = await _access.CheckParticipantAsync(code, userId);
            if (participant.Result == AccessResult.NoActiveSession)
            {
                throw new HubException("Esta sala não tem uma sessão ativa.");
            }
            if (participant.Result != AccessResult.Allowed)
            {
                throw new HubException("Entra na sala antes de receberes as atualizações.");
            }
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, code.Trim());
    }

    public Task LeaveRoom(string code) => Groups.RemoveFromGroupAsync(Context.ConnectionId, code.Trim());
}
