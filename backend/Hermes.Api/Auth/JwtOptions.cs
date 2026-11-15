namespace Hermes.Api.Auth;

// Lida da secção "Jwt" do appsettings.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // Chave de assinatura (mínimo 32 caracteres). Em produção vem de variável de ambiente.
    public string Key { get; set; } = string.Empty;

    public int ExpiresMinutes { get; set; } = 480;
}
