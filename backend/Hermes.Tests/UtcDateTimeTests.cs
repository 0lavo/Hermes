using Hermes.Api.Data;
using Hermes.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hermes.Tests;

public class UtcDateTimeTests
{
    [Fact]
    public void Conversor_MarcaADataLidaComoUtc()
    {
        var lida = new UtcDateTimeConverter().ConvertFromProvider(new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal(DateTimeKind.Utc, ((DateTime)lida!).Kind);
    }

    [Fact]
    public void Conversor_AplicaSeTambemAsDatasOpcionais()
    {
        var options = new DbContextOptionsBuilder<HermesDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new HermesDbContext(options);

        var endedAt = db.Model.FindEntityType(typeof(Session))!.FindProperty(nameof(Session.EndedAt))!;

        Assert.IsType<UtcDateTimeConverter>(endedAt.GetValueConverter());
    }
}
