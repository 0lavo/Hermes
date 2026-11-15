using Hermes.Api.Auth;
using Hermes.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hermes.Tests;

// Arranca a API dentro do teste, com uma base de dados em memória própria.
public class HermesApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"hermes-tests-{Guid.NewGuid()}";

    // A Google simulada: só aceita os tokens registados com Google.Add(...).
    public FakeGoogleTokenValidator Google { get; } = new();

    static HermesApiFactory()
    {
        // Definida antes de a API arrancar: o Program.cs lê a chave logo no arranque.
        Environment.SetEnvironmentVariable("Jwt__Key", "chave-de-testes-hermes-com-mais-de-32-caracteres");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<HermesDbContext>(options => options.UseInMemoryDatabase(_dbName));

            // Troca a validação real da Google pela simulada.
            services.RemoveAll<IGoogleTokenValidator>();
            services.AddSingleton<IGoogleTokenValidator>(Google);
        });
    }
}
