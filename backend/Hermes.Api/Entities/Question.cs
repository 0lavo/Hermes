namespace Hermes.Api.Entities;

public enum QuestionStatus
{
    Pending,
    Answered,
    Removed
}

public class Question
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int AuthorId { get; set; }
    public Participant Author { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public QuestionStatus Status { get; set; } = QuestionStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AnsweredAt { get; set; }

    // O autor conta como 1.º voto: ao criar a dúvida, cria-se também o seu Vote.
    public List<Vote> Votes { get; set; } = new();
}
