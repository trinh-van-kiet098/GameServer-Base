using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace GameServer.Services;

public sealed class TokenService
{
    private readonly ConcurrentDictionary<string, long> _tokens = new();

    public string Issue(long accountId)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        _tokens[token] = accountId;
        return token;
    }

    public bool TryResolve(string token, out long accountId)
        => _tokens.TryGetValue(token, out accountId);
}