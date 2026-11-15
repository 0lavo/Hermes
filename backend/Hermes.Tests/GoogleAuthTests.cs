using System.Net;
using System.Net.Http.Json;
using Hermes.Api.Auth;
using Hermes.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Hermes.Tests;

public class GoogleAuthTests : IClassFixture<HermesApiFactory>
{
    private readonly HermesApiFactory _factory;

    public GoogleAuthTests(HermesApiFactory factory)
    {
        _factory = factory;
    }

    private static string Unique() => Guid.NewGuid().ToString("N");

    // Regista na Google simulada um token válido e devolve-o com os dados do utilizador.
    private (string Token, GoogleUserInfo Info) NewGoogleToken(bool emailVerified = true, string? email = null)
    {
        var token = $"token-{Unique()}";
        var info = new GoogleUserInfo(
            Subject: $"sub-{Unique()}",
            Email: email ?? $"{Unique()}@gmail.com",
            Name: "Utilizador Google",
            EmailVerified: emailVerified);

        _factory.Google.Add(token, info);
        return (token, info);
    }

    private static Task<HttpResponseMessage> GoogleLoginAsync(HttpClient client, string token) =>
        client.PostAsJsonAsync("/api/auth/google", new { idToken = token });

    [Fact]
    public async Task Google_ContaNova_Devolve201ECriaUtilizadorSemPassword()
    {
        var client = _factory.CreateClient();
        var (token, info) = NewGoogleToken();

        var response = await GoogleLoginAsync(client, token);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal(info.Email, body.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HermesDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == info.Email);
        Assert.Equal(info.Subject, user.GoogleSubject);
        Assert.Null(user.PasswordHash);
    }

    [Fact]
    public async Task Google_ContaExistente_Devolve200ComOMesmoUtilizador()
    {
        var client = _factory.CreateClient();
        var (token, _) = NewGoogleToken();

        var first = await GoogleLoginAsync(client, token);
        var second = await GoogleLoginAsync(client, token);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstBody = await first.Content.ReadFromJsonAsync<AuthResponse>();
        var secondBody = await second.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(firstBody);
        Assert.NotNull(secondBody);
        Assert.Equal(firstBody.User.Id, secondBody.User.Id);
    }

    [Fact]
    public async Task Google_TokenInvalido_Devolve401()
    {
        var client = _factory.CreateClient();

        var response = await GoogleLoginAsync(client, "token-que-a-google-nao-conhece");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Google_EmailNaoVerificado_Devolve401()
    {
        var client = _factory.CreateClient();
        var (token, _) = NewGoogleToken(emailVerified: false);

        var response = await GoogleLoginAsync(client, token);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Google_EmailJaRegistadoPorPassword_Devolve409()
    {
        var client = _factory.CreateClient();
        var email = $"{Unique()}@hermes.pt";
        await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Teste", email, password = "Hermes2026" });
        var (token, _) = NewGoogleToken(email: email);

        var response = await GoogleLoginAsync(client, token);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ContaCriadaSoComGoogle_NaoEntraPorPassword()
    {
        var client = _factory.CreateClient();
        var (token, info) = NewGoogleToken();
        await GoogleLoginAsync(client, token);

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = info.Email, password = "Hermes2026" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Google_SemIdToken_Devolve400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/google", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
