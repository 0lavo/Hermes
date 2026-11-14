namespace Hermes.Api.Entities;

public class Room
{
    public int Id { get; set; }

    // Código único partilhado com a turma (ex.: AB12CD).
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Uma sala tem várias sessões (aulas), mas só uma ativa de cada vez.
    public List<Session> Sessions { get; set; } = new();
}
