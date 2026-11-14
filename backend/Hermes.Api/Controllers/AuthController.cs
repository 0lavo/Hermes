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

    public AuthController(HermesDbContext db, IJwtTokenService tokens)
    {
        _db = db;
        _tokens = tokens;
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

    // Com JWT o servidor não guarda sessão: o cliente apaga o token.
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout() => NoContent();

    private AuthResponse ToResponse(User user) =>
        new(new UserDto(user.Id, user.Name, user.Email), _tokens.CreateToken(user));
}
