namespace Hermes.Api.Entities;

// Um participante só pode ter um voto por dúvida (índice único a configurar no DbContext).
public class Vote
{
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
