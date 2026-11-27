using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hermes.Api.Auth;
using Hermes.Api.Rooms;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace Hermes.Tests;

// Hub de tempo real (US14): quem pode entrar no grupo da sala e o aviso de sessão encerrada.
public class RoomHubTests : IClassFixture<HermesApiFactory>
{
    private readonly HermesApiFactory _factory;

    public RoomHubTests(HermesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task End_AvisaOsAlunosDaSala()
    {
        var (owner, _) = await NewUserAsync();
        var (student, studentToken) = await NewUserAsync();
        var room = await CreateActiveRoomAsync(owner);
        (await student.PostAsJsonAsync($"/api/sessions/{room.Code}/join", new { anonymous = true })).EnsureSuccessStatusCode();

        await using var hub = await ConnectAsync(studentToken);
        var ended = new TaskCompletionSource<string?>();
        hub.On<JsonElement>("SessionEnded", e => ended.TrySetResult(e.GetProperty("code").GetString()));
        await hub.InvokeAsync("JoinRoom", room.Code);

        (await owner.PostAsync($"/api/sessions/{room.Code}/end", null)).EnsureSuccessStatusCode();

        Assert.Equal(room.Code, await ended.Task.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task JoinRoom_SemTerEntradoNaSala_DaHubException()
    {
        var (owner, _) = await NewUserAsync();
        var (_, strangerToken) = await NewUserAsync();
        var room = await CreateActiveRoomAsync(owner);

        await using var hub = await ConnectAsync(strangerToken);

        var error = await Assert.ThrowsAsync<HubException>(() => hub.InvokeAsync("JoinRoom", room.Code));
        Assert.Contains("Entra na sala", error.Message);
    }

    [Fact]
    public async Task Ligar_SemToken_ERecusado()
    {
        await using var hub = BuildConnection("/hubs/rooms");

        await Assert.ThrowsAsync<HttpRequestException>(() => hub.StartAsync());
    }

    // ---------- helpers ----------

    // O token vai no URL, como no browser (o WebSocket não manda cabeçalhos).
    private async Task<HubConnection> ConnectAsync(string token)
    {
        var hub = BuildConnection($"/hubs/rooms?access_token={token}");
        await hub.StartAsync();
        return hub;
    }

    // O servidor de testes é em memória: liga por long polling através do handler dele, sem rede nem WebSocket.
    private HubConnection BuildConnection(string path) =>
        new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, path), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

    private async Task<(HttpClient Client, string Token)> NewUserAsync()
    {
        var client = _factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Teste", email = $"{Guid.NewGuid():N}@hermes.pt", password = "Hermes2026" });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return (client, auth.Token);
    }

    private static async Task<RoomDto> CreateActiveRoomAsync(HttpClient owner)
    {
        var room = (await (await owner.PostAsync("/api/sessions", null)).Content.ReadFromJsonAsync<RoomDto>())!;
        (await owner.PostAsync($"/api/sessions/{room.Code}/start", null)).EnsureSuccessStatusCode();
        return room;
    }
}
