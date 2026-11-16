using System.Security.Claims;

namespace Hermes.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    // Id do utilizador autenticado (claim "sub" do JWT). Devolve null se não houver token válido.
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }
}
