using Microsoft.AspNetCore.SignalR;

namespace Hermes.Api.Hubs;

// Envia os eventos do hub a partir dos controllers, que não estão dentro de uma ligação SignalR.
// O grupo é o código da sala (ver RoomHub.JoinRoom).
public interface IRoomNotifier
{
    Task SessionEndedAsync(string code);
}

public class RoomNotifier : IRoomNotifier
{
    private readonly IHubContext<RoomHub> _hub;

    public RoomNotifier(IHubContext<RoomHub> hub)
    {
        _hub = hub;
    }

    public Task SessionEndedAsync(string code) =>
        _hub.Clients.Group(code).SendAsync("SessionEnded", new { code });
}
