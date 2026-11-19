using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Hermes.Api.Auth;

// Declara o esquema Bearer (JWT) no OpenAPI para o Swagger UI mostrar o botão "Authorize".
public class BearerSecurityTransformer : IOpenApiDocumentTransformer
{
    // Endpoints públicos: não exigem token.
    private static readonly HashSet<string> AnonymousPaths = new()
    {
        "/api/auth/register",
        "/api/auth/login",
        "/api/auth/google"
    };

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT"
            }
        };

        foreach (var path in document.Paths)
        {
            if (AnonymousPaths.Contains(path.Key) || path.Value.Operations is null)
            {
                continue;
            }

            foreach (var operation in path.Value.Operations.Values)
            {
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }
        }

        return Task.CompletedTask;
    }
}
