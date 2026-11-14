namespace Hermes.Api.Entities;

// Cada entrada de um utilizador numa sessão.
public class Participant
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Escolhido ao entrar: se o nome fica oculto para os colegas.
    public bool IsAnonymous { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public List<Question> Questions { get; set; } = new();
    public List<Vote> Votes { get; set; } = new();
}
