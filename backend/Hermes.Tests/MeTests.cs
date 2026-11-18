using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hermes.Api.Auth;
using Xunit;

namespace Hermes.Tests;

public class MeTests : IClassFixture<HermesApiFactory>
{
    private readonly HermesApiFactory _factory;

    public MeTests(HermesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Me_SemToken_Devolve401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComTokenFalso_Devolve401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "isto.nao.e-um-token");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComTokenValido_DevolveOUtilizadorDoToken()
    {
        var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@hermes.pt";
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Teste", email, password = "Hermes2026" });
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var me = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(me);
        Assert.Equal(auth.User.Id, me.Id);
        Assert.Equal(email, me.Email);
    }
}
