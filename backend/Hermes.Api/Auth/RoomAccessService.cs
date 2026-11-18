using Hermes.Api.Data;
using Hermes.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Api.Auth;

public enum AccessResult
{
    Allowed,
    RoomNotFound,      // 404: o código não existe
    NotOwner,          // 403: não é o dono da sala
    NoActiveSession,   // 409: a sala não tem sessão ativa
    NotParticipant     // 403: não entrou na sessão ativa (o dono nunca é participante)
}

public record OwnerAccess(AccessResult Result, Room? Room);

public record ParticipantAccess(AccessResult Result, Session? Session, Participant? Participant);

// Decide se um utilizador pode agir numa sala. O servidor valida sempre, nunca só a interface (RNF14).
public interface IRoomAccessService
{
    // Para operações do dono: iniciar/encerrar sessão, marcar dúvida como respondida.
    Task<OwnerAccess> CheckOwnerAsync(string roomCode, int userId);

    // Para operações de aluno: submeter dúvida, votar. Exige sessão ativa.
    Task<ParticipantAccess> CheckParticipantAsync(string roomCode, int userId);
}

public class RoomAccessService : IRoomAccessService
{
    private readonly HermesDbContext _db;

    public RoomAccessService(HermesDbContext db)
    {
        _db = db;
    }

    public async Task<OwnerAccess> CheckOwnerAsync(string roomCode, int userId)
    {
        var room = await FindRoomAsync(roomCode);

        if (room is null)
        {
            return new OwnerAccess(AccessResult.RoomNotFound, null);
        }

        if (room.OwnerId != userId)
        {
            return new OwnerAccess(AccessResult.NotOwner, null);
        }

        return new OwnerAccess(AccessResult.Allowed, room);
    }

    public async Task<ParticipantAccess> CheckParticipantAsync(string roomCode, int userId)
    {
        var room = await FindRoomAsync(roomCode);

        if (room is null)
        {
            return new ParticipantAccess(AccessResult.RoomNotFound, null, null);
        }

        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.RoomId == room.Id && s.Status == SessionStatus.Active);

        if (session is null)
        {
            return new ParticipantAccess(AccessResult.NoActiveSession, null, null);
        }

        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.SessionId == session.Id && p.UserId == userId);

        if (participant is null)
        {
            return new ParticipantAccess(AccessResult.NotParticipant, session, null);
        }

        return new ParticipantAccess(AccessResult.Allowed, session, participant);
    }

    // Os códigos de sala guardam-se em maiúsculas (ex.: AB12CD).
    private Task<Room?> FindRoomAsync(string roomCode)
    {
        var code = roomCode.Trim().ToUpperInvariant();
        return _db.Rooms.FirstOrDefaultAsync(r => r.Code == code);
    }
}
