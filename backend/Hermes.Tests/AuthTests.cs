using System.Net;
using System.Net.Http.Json;
using Hermes.Api.Auth;
using Xunit;

namespace Hermes.Tests;

public class AuthTests : IClassFixture<HermesApiFactory>
{
    private readonly HermesApiFactory _factory;

    public AuthTests(HermesApiFactory factory)
    {
        _factory = factory;
    }

    // Cada teste usa um e-mail único porque a base de dados é partilhada dentro da classe.
    private static string NewEmail() => $"{Guid.NewGuid():N}@hermes.pt";

    [Fact]
    public async Task Register_ComDadosValidos_Devolve201ComTokenEUtilizador()
    {
        var client = _factory.CreateClient();
        var email = NewEmail();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { name = "Teste", email, password = "Hermes2026" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.Equal(email, body.User.Email);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
    }
}
