namespace Hermes.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Nulo quando a conta foi criada só com Google.
    public string? PasswordHash { get; set; }

    // Identificador da conta Google ("sub"); nulo se não usar Google.
    public string? GoogleSubject { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Room> OwnedRooms { get; set; } = new();
    public List<Participant> Participations { get; set; } = new();
}
