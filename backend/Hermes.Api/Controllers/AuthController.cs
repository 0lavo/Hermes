using Hermes.Api.Auth;
using Hermes.Api.Data;
using Hermes.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly HermesDbContext _db;
    private readonly IJwtTokenService _tokens;
    private readonly IGoogleTokenValidator _google;

    public AuthController(HermesDbContext db, IJwtTokenService tokens, IGoogleTokenValidator google)
    {
        _db = db;
        _tokens = tokens;
        _google = google;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "Este e-mail já está registado." });
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Mesma resposta para e-mail inexistente e password errada (não revelar que e-mails existem).
        // Contas criadas só com Google não têm hash e não entram por password.
        if (user?.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "E-mail ou password incorretos." });
        }

        return Ok(ToResponse(user));
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
    {
        var info = await _google.ValidateAsync(request.IdToken);

        if (info is null)
        {
            return Unauthorized(new { message = "Token do Google inválido ou expirado." });
        }

        if (!info.EmailVerified)
        {
            return Unauthorized(new { message = "O e-mail da conta Google não está verificado." });
        }

        // Conta já associada a este utilizador Google: entra.
        var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleSubject == info.Subject);
        if (user is not null)
        {
            return Ok(ToResponse(user));
        }

        var email = info.Email.Trim().ToLowerInvariant();

        // Já existe uma conta por password com este e-mail: não se liga automaticamente,
        // porque o registo por password não confirma o e-mail.
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "Já existe uma conta com este e-mail. Entra com e-mail e password." });
        }

        var name = info.Name.Trim();
        user = new User
        {
            Name = name.Length > 100 ? name[..100] : name,
            Email = email,
            GoogleSubject = info.Subject
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    // Devolve o utilizador do token (o frontend usa-o para recuperar a sessão).
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.GetUserId();
        if (id is null)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(id.Value);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserDto(user.Id, user.Name, user.Email));
    }

    // Com JWT o servidor não guarda sessão: o cliente apaga o token.
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout() => NoContent();

    private AuthResponse ToResponse(User user) =>
        new(new UserDto(user.Id, user.Name, user.Email), _tokens.CreateToken(user));
}
