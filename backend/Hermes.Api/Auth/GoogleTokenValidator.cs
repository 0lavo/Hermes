using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Hermes.Api.Auth;

// Lida da secção "Google" do appsettings.
public class GoogleOptions
{
    public const string SectionName = "Google";

    public string ClientId { get; set; } = string.Empty;
}

public record GoogleUserInfo(string Subject, string Email, string Name, bool EmailVerified);

public interface IGoogleTokenValidator
{
    // Devolve null se o token for inválido, expirado ou não for destinado a esta aplicação.
    Task<GoogleUserInfo?> ValidateAsync(string idToken);
}

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleOptions _options;

    public GoogleTokenValidator(IOptions<GoogleOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                // O token só é aceite se tiver sido emitido para o nosso Client ID.
                Audience = new[] { _options.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                payload.Name ?? payload.Email,
                payload.EmailVerified);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }
}
