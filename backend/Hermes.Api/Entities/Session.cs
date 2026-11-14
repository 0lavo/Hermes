namespace Hermes.Api.Entities;

public enum SessionStatus
{
    Active,
    Ended
}

// Uma aula numa sala. Participantes, dúvidas e votos pertencem à sessão.
public class Session
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public SessionStatus Status { get; set; } = SessionStatus.Active;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    public List<Participant> Participants { get; set; } = new();
    public List<Question> Questions { get; set; } = new();
}
