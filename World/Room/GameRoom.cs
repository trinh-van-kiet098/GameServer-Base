using GameServer.Core;
using GameServer.Network;
using GameServer.World.Entity;

namespace GameServer.World.Room;

public sealed class GameRoom
{
    public Guid RoomId { get; } = Guid.NewGuid();
    public RoomState State { get; private set; } = RoomState.Waiting;

    private readonly List<Player> _slots = new();
    private readonly int _maxSlots;
    private readonly Action<GameRoom> _onFinished;
    private long _startedAtMs;
    private const int MatchDurationMs = 5 * 60 * 1000;

    public GameRoom(int maxSlots, Action<GameRoom> onFinished)
    {
        _maxSlots = maxSlots;
        _onFinished = onFinished;
    }

    public void Tick(long nowMs)
    {
        switch (State)
        {
            case RoomState.Waiting:
                if (_slots.Count == _maxSlots) StartMatch(nowMs);
                break;

            case RoomState.Playing:
                UpdateCombat(nowMs);
                if (nowMs - _startedAtMs >= MatchDurationMs)
                    EndMatch(DetermineWinner());
                break;

            case RoomState.Finished:
                break;
        }
    }

    public void AddPlayer(Player p)
    {
        if (_slots.Count >= _maxSlots) return;
        _slots.Add(p);
        Broadcast(Packet.String(Opcode.S2C_PlayerJoined, p.Name));
    }

    public void HandleAction(Player p, Packet packet)
    {
        p.Score += 1;
    }

    private void StartMatch(long nowMs)
    {
        State = RoomState.Playing;
        _startedAtMs = nowMs;
        Broadcast(Packet.Empty(Opcode.S2C_MatchStart));
        Logger.Info($"room {RoomId:N} match start");
    }

    private void UpdateCombat(long nowMs)
    {
        // Demo: tu dong ket thuc sau 60s
        if (nowMs - _startedAtMs >= 60_000)
            EndMatch(DetermineWinner());
    }

    private Player DetermineWinner()
        => _slots.OrderByDescending(p => p.Score).First();

    private void EndMatch(Player winner)
    {
        State = RoomState.Finished;
        foreach (var p in _slots)
        {
            var win = p == winner;
            p.Session.Send(Packet.String(Opcode.S2C_MatchResult, win ? "WIN" : "LOSE"));
        }
        Logger.Info($"room {RoomId:N} finished, winner={winner.Name}");
        _onFinished(this);
    }

    private void Broadcast(Packet p)
    {
        foreach (var pl in _slots) pl.Session.Send(p);
    }
}