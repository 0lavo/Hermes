namespace Hermes.Api.Entities;

// Uma aula numa sala. Participantes, dúvidas e votos pertencem à sessão.
public class Session
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    // null = sessão ativa; o "end" preenche-o e a sessão fica encerrada.
    public DateTime? EndedAt { get; set; }

    public List<Participant> Participants { get; set; } = new();
    public List<Question> Questions { get; set; } = new();
}
