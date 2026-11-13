namespace Hermes.Api.Entities;

public enum RoomStatus
{
    Created,
    Active,
    Ended
}

public class Room
{
    public int Id { get; set; }

    // Código único partilhado com a turma (ex.: AB12CD).
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public RoomStatus Status { get; set; } = RoomStatus.Created;

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public List<Participant> Participants { get; set; } = new();
    public List<Question> Questions { get; set; } = new();
}
