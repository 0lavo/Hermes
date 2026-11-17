using Hermes.Api.Auth;
using Hermes.Api.Data;
using Hermes.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hermes.Tests;

public class RoomAccessServiceTests
{
    private static HermesDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<HermesDbContext>()
            .UseInMemoryDatabase($"access-tests-{Guid.NewGuid()}")
            .Options;
        return new HermesDbContext(options);
    }

    // Cria um dono, um aluno e uma sala (código AB12CD).
    private static async Task<(User Owner, User Student, Room Room)> SeedRoomAsync(HermesDbContext db)
    {
        var owner = new User { Name = "Dono", Email = $"{Guid.NewGuid():N}@hermes.pt" };
        var student = new User { Name = "Aluno", Email = $"{Guid.NewGuid():N}@hermes.pt" };
        var room = new Room { Code = "AB12CD", Name = "Biologia", Owner = owner };

        db.AddRange(owner, student, room);
        await db.SaveChangesAsync();

        return (owner, student, room);
    }

    private static async Task<Session> AddSessionAsync(HermesDbContext db, Room room, SessionStatus status)
    {
        var session = new Session
        {
            RoomId = room.Id,
            Status = status,
            EndedAt = status == SessionStatus.Ended ? DateTime.UtcNow : null
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        return session;
    }

    private static async Task<Participant> JoinAsync(HermesDbContext db, Session session, User user)
    {
        var participant = new Participant { SessionId = session.Id, UserId = user.Id, IsAnonymous = true };

        db.Participants.Add(participant);
        await db.SaveChangesAsync();

        return participant;
    }

    // ---------- Dono ----------

    [Fact]
    public async Task Owner_CodigoInexistente_DevolveRoomNotFound()
    {
        using var db = NewDb();
        var service = new RoomAccessService(db);

        var result = await service.CheckOwnerAsync("ZZZZZZ", 1);

        Assert.Equal(AccessResult.RoomNotFound, result.Result);
        Assert.Null(result.Room);
    }

    [Fact]
    public async Task Owner_UtilizadorQueNaoEDono_DevolveNotOwner()
    {
        using var db = NewDb();
        var (_, student, room) = await SeedRoomAsync(db);
        var service = new RoomAccessService(db);

        var result = await service.CheckOwnerAsync(room.Code, student.Id);

        Assert.Equal(AccessResult.NotOwner, result.Result);
        Assert.Null(result.Room);
    }

    [Fact]
    public async Task Owner_Dono_DevolveAllowedComASala()
    {
        using var db = NewDb();
        var (owner, _, room) = await SeedRoomAsync(db);
        var service = new RoomAccessService(db);

        var result = await service.CheckOwnerAsync(room.Code, owner.Id);

        Assert.Equal(AccessResult.Allowed, result.Result);
        Assert.NotNull(result.Room);
        Assert.Equal(room.Id, result.Room.Id);
    }

    [Fact]
    public async Task Owner_CodigoEmMinusculas_DevolveAllowed()
    {
        using var db = NewDb();
        var (owner, _, room) = await SeedRoomAsync(db);
        var service = new RoomAccessService(db);

        var result = await service.CheckOwnerAsync(room.Code.ToLowerInvariant(), owner.Id);

        Assert.Equal(AccessResult.Allowed, result.Result);
    }

    // ---------- Participante ----------

    [Fact]
    public async Task Participant_SalaInexistente_DevolveRoomNotFound()
    {
        using var db = NewDb();
        var service = new RoomAccessService(db);

        var result = await service.CheckParticipantAsync("ZZZZZZ", 1);

        Assert.Equal(AccessResult.RoomNotFound, result.Result);
    }

    [Fact]
    public async Task Participant_SalaSemSessaoAtiva_DevolveNoActiveSession()
    {
        using var db = NewDb();
        var (_, student, room) = await SeedRoomAsync(db);
        var ended = await AddSessionAsync(db, room, SessionStatus.Ended);
        await JoinAsync(db, ended, student);
        var service = new RoomAccessService(db);

        var result = await service.CheckParticipantAsync(room.Code, student.Id);

        Assert.Equal(AccessResult.NoActiveSession, result.Result);
    }

    [Fact]
    public async Task Participant_AlunoQueNaoEntrou_DevolveNotParticipant()
    {
        using var db = NewDb();
        var (_, student, room) = await SeedRoomAsync(db);
        await AddSessionAsync(db, room, SessionStatus.Active);
        var service = new RoomAccessService(db);

        var result = await service.CheckParticipantAsync(room.Code, student.Id);

        Assert.Equal(AccessResult.NotParticipant, result.Result);
    }

    [Fact]
    public async Task Participant_Dono_DevolveNotParticipant()
    {
        using var db = NewDb();
        var (owner, _, room) = await SeedRoomAsync(db);
        await AddSessionAsync(db, room, SessionStatus.Active);
        var service = new RoomAccessService(db);

        var result = await service.CheckParticipantAsync(room.Code, owner.Id);

        Assert.Equal(AccessResult.NotParticipant, result.Result);
    }

    [Fact]
    public async Task Participant_AlunoQueEntrou_DevolveAllowedComSessaoEParticipante()
    {
        using var db = NewDb();
        var (_, student, room) = await SeedRoomAsync(db);
        var session = await AddSessionAsync(db, room, SessionStatus.Active);
        var joined = await JoinAsync(db, session, student);
        var service = new RoomAccessService(db);

        var result = await service.CheckParticipantAsync(room.Code, student.Id);

        Assert.Equal(AccessResult.Allowed, result.Result);
        Assert.NotNull(result.Session);
        Assert.NotNull(result.Participant);
        Assert.Equal(session.Id, result.Session.Id);
        Assert.Equal(joined.Id, result.Participant.Id);
    }

    [Fact]
    public async Task Participant_AlunoDeSessaoAnterior_NaoEParticipanteDaSessaoNova()
    {
        using var db = NewDb();
        var (_, student, room) = await SeedRoomAsync(db);
        var old = await AddSessionAsync(db, room, SessionStatus.Ended);
        await JoinAsync(db, old, student);
        await AddSessionAsync(db, room, SessionStatus.Active);
        var service = new RoomAccessService(db);

        var result = await service.CheckParticipantAsync(room.Code, student.Id);

        Assert.Equal(AccessResult.NotParticipant, result.Result);
    }
}
