using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hermes.Api.Auth;
using Hermes.Api.Rooms;
using Xunit;

namespace Hermes.Tests;

// Salas e sessões (US03, US09): criar, iniciar, encerrar, entrar e listar.
public class SessionsTests : IClassFixture<HermesApiFactory>
{
    private readonly HermesApiFactory _factory;

    public SessionsTests(HermesApiFactory factory)
    {
        _factory = factory;
    }

    // ---------- POST /api/sessions ----------

    [Fact]
    public async Task Create_SemToken_Devolve401()
    {
        var response = await _factory.CreateClient().PostAsync("/api/sessions", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_SemCorpo_CriaSalaComCodigoDe6DigitosENomePorDefeito()
    {
        var owner = await NewUserClientAsync();

        var response = await owner.PostAsync("/api/sessions", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var room = await response.Content.ReadFromJsonAsync<RoomDto>();
        Assert.NotNull(room);
        Assert.Matches("^[1-9][0-9]{5}$", room.Code);
        Assert.Equal($"Sala {room.Code}", room.Name);
        Assert.Equal("created", room.Status);
    }

    [Fact]
    public async Task Create_ComNome_UsaONome()
    {
        var owner = await NewUserClientAsync();

        var room = await CreateRoomAsync(owner, "LDP - Turma A");

        Assert.Equal("LDP - Turma A", room.Name);
    }

    // ---------- POST /api/sessions/{code}/start ----------

    [Fact]
    public async Task Start_DoDono_AtivaASala_ESegundoStartDevolve409()
    {
        var owner = await NewUserClientAsync();
        var room = await CreateRoomAsync(owner);

        var first = await owner.PostAsync($"/api/sessions/{room.Code}/start", null);
        var second = await owner.PostAsync($"/api/sessions/{room.Code}/start", null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal("active", (await first.Content.ReadFromJsonAsync<RoomDto>())!.Status);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Start_DeOutroUtilizador_Devolve403()
    {
        var owner = await NewUserClientAsync();
        var other = await NewUserClientAsync();
        var room = await CreateRoomAsync(owner);

        var response = await other.PostAsync($"/api/sessions/{room.Code}/start", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Start_SalaInexistente_Devolve404()
    {
        var owner = await NewUserClientAsync();

        // Os códigos gerados vão de 100000 a 999999, por isso este nunca existe.
        var response = await owner.PostAsync("/api/sessions/000000/start", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- POST /api/sessions/{code}/end ----------

    [Fact]
    public async Task End_SemSessaoAtiva_Devolve409()
    {
        var owner = await NewUserClientAsync();
        var room = await CreateRoomAsync(owner);

        var response = await owner.PostAsync($"/api/sessions/{room.Code}/end", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task End_DepoisDoStart_EncerraASala_ENaoSePodeReiniciar()
    {
        var owner = await NewUserClientAsync();
        var room = await CreateActiveRoomAsync(owner);

        var end = await owner.PostAsync($"/api/sessions/{room.Code}/end", null);
        var restart = await owner.PostAsync($"/api/sessions/{room.Code}/start", null);

        Assert.Equal(HttpStatusCode.OK, end.StatusCode);
        Assert.Equal("ended", (await end.Content.ReadFromJsonAsync<RoomDto>())!.Status);
        Assert.Equal(HttpStatusCode.Conflict, restart.StatusCode);
    }

    // ---------- POST /api/sessions/{code}/join ----------

    [Fact]
    public async Task Join_SemAnonymous_Devolve400()
    {
        var owner = await NewUserClientAsync();
        var student = await NewUserClientAsync();
        var room = await CreateActiveRoomAsync(owner);

        var response = await student.PostAsJsonAsync($"/api/sessions/{room.Code}/join", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Join_DoDono_Devolve403()
    {
        var owner = await NewUserClientAsync();
        var room = await CreateActiveRoomAsync(owner);

        var response = await owner.PostAsJsonAsync($"/api/sessions/{room.Code}/join", new { anonymous = true });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Join_SemSessaoAtiva_Devolve409()
    {
        var owner = await NewUserClientAsync();
        var student = await NewUserClientAsync();
        var room = await CreateRoomAsync(owner);

        var response = await student.PostAsJsonAsync($"/api/sessions/{room.Code}/join", new { anonymous = true });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Join_DuasVezes_AtualizaAEscolha()
    {
        var owner = await NewUserClientAsync();
        var student = await NewUserClientAsync();
        var room = await CreateActiveRoomAsync(owner);

        var first = await student.PostAsJsonAsync($"/api/sessions/{room.Code}/join", new { anonymous = true });
        var second = await student.PostAsJsonAsync($"/api/sessions/{room.Code}/join", new { anonymous = false });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.True((await first.Content.ReadFromJsonAsync<JoinResponse>())!.Anonymous);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.False((await second.Content.ReadFromJsonAsync<JoinResponse>())!.Anonymous);
    }

    // ---------- GET /api/sessions ----------

    [Fact]
    public async Task MyRooms_DevolveSoAsSalasDoDonoComOEstadoCalculado()
    {
        var owner = await NewUserClientAsync();
        var other = await NewUserClientAsync();
        var created = await CreateRoomAsync(owner);
        var active = await CreateActiveRoomAsync(owner);
        var ended = await CreateActiveRoomAsync(owner);
        (await owner.PostAsync($"/api/sessions/{ended.Code}/end", null)).EnsureSuccessStatusCode();
        await CreateRoomAsync(other);

        var rooms = await owner.GetFromJsonAsync<List<RoomDto>>("/api/sessions");

        Assert.NotNull(rooms);
        var status = rooms.ToDictionary(r => r.Code, r => r.Status);
        Assert.Equal(3, status.Count);
        Assert.Equal("created", status[created.Code]);
        Assert.Equal("active", status[active.Code]);
        Assert.Equal("ended", status[ended.Code]);
    }

    // ---------- helpers ----------

    // Regista um utilizador novo e devolve um cliente já autenticado com o token dele.
    private async Task<HttpClient> NewUserClientAsync()
    {
        var client = _factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Teste", email = $"{Guid.NewGuid():N}@hermes.pt", password = "Hermes2026" });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    private static async Task<RoomDto> CreateRoomAsync(HttpClient owner, string? name = null)
    {
        var response = await owner.PostAsJsonAsync("/api/sessions", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RoomDto>())!;
    }

    private static async Task<RoomDto> CreateActiveRoomAsync(HttpClient owner)
    {
        var room = await CreateRoomAsync(owner);
        (await owner.PostAsync($"/api/sessions/{room.Code}/start", null)).EnsureSuccessStatusCode();
        return room;
    }
}
