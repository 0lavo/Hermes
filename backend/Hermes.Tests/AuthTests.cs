using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hermes.Api.Auth;
using Hermes.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hermes.Tests;

public class AuthTests : IClassFixture<HermesApiFactory>
{
    private const string ValidPassword = "Hermes2026";
    private readonly HermesApiFactory _factory;

    public AuthTests(HermesApiFactory factory)
    {
        _factory = factory;
    }

    // Cada teste usa um e-mail único porque a base de dados é partilhada dentro da classe.
    private static string NewEmail() => $"{Guid.NewGuid():N}@hermes.pt";

    private static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string password = ValidPassword) =>
        client.PostAsJsonAsync("/api/auth/register", new { name = "Teste", email, password });

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password });

    // ---------- Registo ----------

    [Fact]
    public async Task Register_ComDadosValidos_Devolve201ComTokenEUtilizador()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();

        var response = await RegisterAsync(client, email);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal(email, body.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
    }

    [Fact]
    public async Task Register_EmailRepetidoMesmoComMaiusculas_Devolve409()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();

        await RegisterAsync(client, email);
        var response = await RegisterAsync(client, email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_PasswordCurta_Devolve400()
    {
        var client = _factory.CreateClient();

        var response = await RegisterAsync(client, NewEmail(), "123");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_EmailInvalido_Devolve400()
    {
        var client = _factory.CreateClient();

        var response = await RegisterAsync(client, "isto-nao-e-um-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_GuardaEmailEmMinusculasEPasswordSoComoHash()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();

        await RegisterAsync(client, email.ToUpperInvariant());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HermesDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == email);

        Assert.NotNull(user.PasswordHash);
        Assert.NotEqual(ValidPassword, user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(ValidPassword, user.PasswordHash));
    }

    // ---------- Login ----------

    [Fact]
    public async Task Login_ComCredenciaisCorretas_Devolve200ComToken()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();
        await RegisterAsync(client, email);

        var response = await LoginAsync(client, email, ValidPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
    }

    [Fact]
    public async Task Login_PasswordErrada_Devolve401()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();
        await RegisterAsync(client, email);

        var response = await LoginAsync(client, email, "errada12345");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmailInexistente_Devolve401()
    {
        var client = _factory.CreateClient();

        var response = await LoginAsync(client, NewEmail(), ValidPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- Logout ----------

    [Fact]
    public async Task Logout_SemToken_Devolve401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ComToken_Devolve204()
    {
        var client = _factory.CreateClient();
        var registerResponse = await RegisterAsync(client, NewEmail());
        var body = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
        var response = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
