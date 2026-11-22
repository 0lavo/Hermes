using System.Security.Cryptography;
using Hermes.Api.Auth;
using Hermes.Api.Data;
using Hermes.Api.Entities;
using Hermes.Api.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Api.Controllers;

// Salas e sessões (US03, US09). A rota segue o contrato da API: /api/sessions.
[ApiController]
[Authorize]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly HermesDbContext _db;
    private readonly IRoomAccessService _access;

    public SessionsController(HermesDbContext db, IRoomAccessService access)
    {
        _db = db;
        _access = access;
    }

    // Cria uma sala; quem a cria passa a ser o dono (RF03). Fica "created" até ao start.
    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        // Allow: o corpo é opcional, um POST sem corpo também cria a sala.
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CreateRoomRequest? request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var code = await NewRoomCodeAsync();
        var name = string.IsNullOrWhiteSpace(request?.Name) ? $"Sala {code}" : request.Name.Trim();

        var room = new Room { Code = code, Name = name, OwnerId = userId.Value };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new RoomDto(room.Code, room.Name, "created"));
    }

    // As salas de que o utilizador é dono, das mais recentes para as mais antigas ("As minhas salas").
    // Fora do contrato original: acrescentado para a Tarefa 8.
    [HttpGet]
    [ProducesResponseType(typeof(List<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MyRooms()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        // O estado é calculado a partir das sessões, não tem coluna na BD.
        var rooms = await _db.Rooms
            .Where(r => r.OwnerId == userId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RoomDto(
                r.Code,
                r.Name,
                !r.Sessions.Any() ? "created"
                    : r.Sessions.Any(s => s.EndedAt == null) ? "active"
                    : "ended"))
            .ToListAsync();

        return Ok(rooms);
    }

    // Inicia a sessão da sala (só o dono). Pelo contrato, uma sala só se inicia uma vez:
    // encerrar é definitivo até ao Sprint 3 (reabrir é o RF30).
    [HttpPost("{code}/start")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Start(string code)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var access = await _access.CheckOwnerAsync(code, userId.Value);
        if (access.Room is null)
        {
            return OwnerAccessError(access.Result);
        }
        var room = access.Room;

        if (await _db.Sessions.AnyAsync(s => s.RoomId == room.Id))
        {
            return Conflict(new ErrorResponse("Esta sala já foi iniciada."));
        }

        _db.Sessions.Add(new Session { RoomId = room.Id });
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Dois starts ao mesmo tempo: o índice único filtrado (1 sessão ativa por sala) trava o segundo.
            // Só acontece no SQL Server; a BD em memória dos testes não tem esse índice.
            return Conflict(new ErrorResponse("Esta sala já foi iniciada."));
        }

        return Ok(new RoomDto(room.Code, room.Name, "active"));
    }

    // Encerra a sessão ativa (só o dono). A partir daqui não entram alunos, dúvidas nem votos.
    [HttpPost("{code}/end")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> End(string code)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var access = await _access.CheckOwnerAsync(code, userId.Value);
        if (access.Room is null)
        {
            return OwnerAccessError(access.Result);
        }
        var room = access.Room;

        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.RoomId == room.Id && s.EndedAt == null);
        if (session is null)
        {
            return Conflict(new ErrorResponse("Esta sala não tem uma sessão ativa."));
        }

        session.EndedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // TODO Tarefa 10: avisar os alunos da sala pelo Hub SignalR que a sessão foi encerrada.

        return Ok(new RoomDto(room.Code, room.Name, "ended"));
    }

    // Um aluno entra na sessão ativa e escolhe se fica anónimo (US09, RF06).
    // A escolha é obrigatória sempre que entra e pode mudar durante a sessão: se já é participante,
    // grava a escolha nova. As dúvidas guardam a escolha do momento em que foram criadas (Question.IsAnonymous).
    [HttpPost("{code}/join")]
    [ProducesResponseType(typeof(JoinResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Join(string code, JoinRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        // Não usa o CheckParticipantAsync: aqui o participante ainda não existe, é este endpoint que o cria.
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Code == code.Trim());
        if (room is null)
        {
            return NotFound(new ErrorResponse("Sala não encontrada."));
        }

        if (room.OwnerId == userId.Value)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResponse("O dono da sala não pode entrar como participante."));
        }

        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.RoomId == room.Id && s.EndedAt == null);
        if (session is null)
        {
            return Conflict(new ErrorResponse("Esta sala não tem uma sessão ativa."));
        }

        // request.Anonymous nunca é null aqui: o [Required] do JoinRequest já devolveu 400.
        var existing = await _db.Participants
            .FirstOrDefaultAsync(p => p.SessionId == session.Id && p.UserId == userId.Value);
        if (existing is not null)
        {
            existing.IsAnonymous = request.Anonymous!.Value;
            await _db.SaveChangesAsync();
            return Ok(new JoinResponse(existing.IsAnonymous));
        }

        var participant = new Participant
        {
            SessionId = session.Id,
            UserId = userId.Value,
            IsAnonymous = request.Anonymous!.Value
        };
        _db.Participants.Add(participant);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Dois joins ao mesmo tempo (duplo clique): o índice único (SessionId, UserId) trava o segundo,
            // que devolve a escolha gravada pelo primeiro.
            existing = await _db.Participants
                .AsNoTracking()
                .FirstAsync(p => p.SessionId == session.Id && p.UserId == userId.Value);
            return Ok(new JoinResponse(existing.IsAnonymous));
        }

        return Ok(new JoinResponse(participant.IsAnonymous));
    }

    // Mapeamento do docs/permissoes.md: RoomNotFound → 404, NotOwner → 403.
    private IActionResult OwnerAccessError(AccessResult result) => result == AccessResult.RoomNotFound
        ? NotFound(new ErrorResponse("Sala não encontrada."))
        : StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse("Só o dono da sala pode fazer isto."));

    // Código curto e numérico, estilo Kahoot: fácil de ditar e de escrever no telemóvel.
    // shortcut: dois creates ao mesmo tempo podem sortear o mesmo código e o segundo dá 500 (índice único);
    // com 900 000 códigos é muito improvável. Se acontecer, apanhar a DbUpdateException e tentar outra vez.
    private async Task<string> NewRoomCodeAsync()
    {
        string code;
        do
        {
            code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        } while (await _db.Rooms.AnyAsync(r => r.Code == code));
        return code;
    }
}
