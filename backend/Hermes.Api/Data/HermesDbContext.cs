using Hermes.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Api.Data;

public class HermesDbContext : DbContext
{
    public HermesDbContext(DbContextOptions<HermesDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Name).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(200);
            e.Property(u => u.GoogleSubject).HasMaxLength(100);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.GoogleSubject).IsUnique();
        });

        modelBuilder.Entity<Room>(e =>
        {
            // Código de 6 dígitos (estilo Kahoot): char(6), tamanho fixo e só ASCII.
            e.Property(r => r.Code).HasMaxLength(6).IsFixedLength().IsUnicode(false).IsRequired();
            e.Property(r => r.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(r => r.Code).IsUnique();

            e.HasOne(r => r.Owner).WithMany(u => u.OwnedRooms)
                .HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Session>(e =>
        {
            // Só pode haver uma sessão ativa (sem EndedAt) por sala.
            e.HasIndex(s => s.RoomId).IsUnique().HasFilter("[EndedAt] IS NULL");

            e.HasOne(s => s.Room).WithMany(r => r.Sessions)
                .HasForeignKey(s => s.RoomId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Participant>(e =>
        {
            // Um utilizador só entra uma vez em cada sessão.
            e.HasIndex(p => new { p.SessionId, p.UserId }).IsUnique();

            e.HasOne(p => p.Session).WithMany(s => s.Participants)
                .HasForeignKey(p => p.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.User).WithMany(u => u.Participations)
                .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Question>(e =>
        {
            e.Property(q => q.Text).HasMaxLength(500).IsRequired();
            e.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(q => new { q.SessionId, q.Status });

            e.HasOne(q => q.Session).WithMany(s => s.Questions)
                .HasForeignKey(q => q.SessionId).OnDelete(DeleteBehavior.Cascade);
            // Restrict evita vários caminhos em cascata no SQL Server.
            e.HasOne(q => q.Author).WithMany(p => p.Questions)
                .HasForeignKey(q => q.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Vote>(e =>
        {
            // RNF04: um participante só vota uma vez em cada dúvida. A própria chave primária garante-o.
            e.HasKey(v => new { v.QuestionId, v.ParticipantId });

            e.HasOne(v => v.Question).WithMany(q => q.Votes)
                .HasForeignKey(v => v.QuestionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Participant).WithMany(p => p.Votes)
                .HasForeignKey(v => v.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
