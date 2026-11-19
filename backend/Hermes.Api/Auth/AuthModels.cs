using System.ComponentModel.DataAnnotations;

namespace Hermes.Api.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail não é válido.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A password é obrigatória.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A password deve ter entre 8 e 100 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A password é obrigatória.")]
    public string Password { get; set; } = string.Empty;
}

public record UserDto(int Id, string Name, string Email);

public record AuthResponse(UserDto User, string Token);

public class GoogleLoginRequest
{
    [Required(ErrorMessage = "O idToken é obrigatório.")]
    public string IdToken { get; set; } = string.Empty;
}

// Corpo das respostas de erro 401 e 409: { "message": "..." }.
public record ErrorResponse(string Message);
