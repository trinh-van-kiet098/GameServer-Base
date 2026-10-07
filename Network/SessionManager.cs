using System.Collections.Concurrent;

namespace GameServer.Network;

public sealed class SessionManager
{
    private readonly ConcurrentDictionary<Guid, ClientSession> _sessions = new();

    public void Add(ClientSession s) => _sessions[s.SessionId] = s;
    public void Remove(ClientSession s) => _sessions.TryRemove(s.SessionId, out _);
    public ClientSession? Get(Guid id) => _sessions.GetValueOrDefault(id);
    public IEnumerable<ClientSession> All => _sessions.Values;
}