using GameServer.Core;
using GameServer.Network;

namespace GameServer.Services;

public sealed class AuthService
{
    private readonly TokenService _tokens;
    private readonly SessionManager _sessions;
    private readonly Dictionary<string, string> _accounts = new();

    public AuthService(TokenService tokens, SessionManager sessions)
    {
        _tokens = tokens;
        _sessions = sessions;
    }

    public async Task LoginAsync(string user, string pass, ClientSession session)
    {
        await Task.Delay(50);

        if (!_accounts.TryGetValue(user, out var stored) || stored != pass)
        {
            session.Send(Packet.String(Opcode.S2C_LoginFail, "invalid"));
            return;
        }

        var token = _tokens.Issue(accountId: user.GetHashCode());
        session.AccountId = user.GetHashCode();
        session.Player = new World.Entity.Player(session, user);
        session.Send(Packet.String(Opcode.S2C_LoginOk, token));
        Logger.Info($"login ok user={user}");
    }

    public async Task RegisterAsync(string user, string pass, ClientSession session)
    {
        await Task.Delay(50);
        if (_accounts.ContainsKey(user))
        {
            session.Send(Packet.String(Opcode.S2C_LoginFail, "exists"));
            return;
        }
        _accounts[user] = pass;
        session.Send(Packet.String(Opcode.S2C_LoginOk, "registered"));
        Logger.Info($"register ok user={user}");
    }
}