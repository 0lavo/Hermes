using System.Collections.Concurrent;
using Hermes.Api.Auth;

namespace Hermes.Tests;

// Simula a Google nos testes: só aceita os tokens registados com Add().
public class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    private readonly ConcurrentDictionary<string, GoogleUserInfo> _tokens = new();

    public void Add(string idToken, GoogleUserInfo info) => _tokens[idToken] = info;

    public Task<GoogleUserInfo?> ValidateAsync(string idToken) =>
        Task.FromResult<GoogleUserInfo?>(_tokens.TryGetValue(idToken, out var info) ? info : null);
}
