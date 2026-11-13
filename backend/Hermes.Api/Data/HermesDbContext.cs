using Hermes.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Api.Data;

public class HermesDbContext : DbContext
{
    public HermesDbContext(DbContextOptions<HermesDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
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
            e.Property(r => r.Code).HasMaxLength(10).IsRequired();
            e.Property(r => r.Name).HasMaxLength(100).IsRequired();
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(r => r.Code).IsUnique();

            e.HasOne(r => r.Owner).WithMany(u => u.OwnedRooms)
                .HasForeignKey(r => r.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Participant>(e =>
        {
            // Um utilizador só entra uma vez em cada sala.
            e.HasIndex(p => new { p.RoomId, p.UserId }).IsUnique();

            e.HasOne(p => p.Room).WithMany(r => r.Participants)
                .HasForeignKey(p => p.RoomId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.User).WithMany(u => u.Participations)
                .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Question>(e =>
        {
            e.Property(q => q.Text).HasMaxLength(500).IsRequired();
            e.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(q => new { q.RoomId, q.Status });

            e.HasOne(q => q.Room).WithMany(r => r.Questions)
                .HasForeignKey(q => q.RoomId).OnDelete(DeleteBehavior.Cascade);
            // Restrict evita vários caminhos em cascata no SQL Server.
            e.HasOne(q => q.Author).WithMany(p => p.Questions)
                .HasForeignKey(q => q.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Vote>(e =>
        {
            // RNF04: um participante só vota uma vez em cada dúvida.
            e.HasIndex(v => new { v.QuestionId, v.ParticipantId }).IsUnique();

            e.HasOne(v => v.Question).WithMany(q => q.Votes)
                .HasForeignKey(v => v.QuestionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Participant).WithMany(p => p.Votes)
                .HasForeignKey(v => v.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
