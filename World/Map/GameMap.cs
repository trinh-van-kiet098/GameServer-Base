using GameServer.Core;
using GameServer.Network;
using GameServer.World.Entity;

namespace GameServer.World.Map;

public sealed class GameMap
{
    public int MapId { get; }
    private readonly List<Player> _players = new();
    private readonly List<Monster> _monsters = new();
    private readonly MinHeapTimer _timers = new();
    private long _nextBossId = 1;

    public GameMap(int mapId) { MapId = mapId; }

    public void ScheduleBossSpawn(long nowMs)
    {
        _timers.Schedule(nowMs, 10_000, () =>
        {
            var boss = new Boss { Id = _nextBossId++ };
            _monsters.Add(boss);
            Broadcast(Packet.String(Opcode.S2C_BossSpawn, $"boss#{boss.Id}"));
            Logger.Info($"map {MapId} spawn boss #{boss.Id}");
            ScheduleBossSpawn(nowMs + 10_000);
        });
    }

    public void Tick(long nowMs)
    {
        _timers.Tick(nowMs);
        foreach (var m in _monsters) m.UpdateAI(nowMs);
    }

    public bool HasPlayer(Player p) => _players.Contains(p);

    public void AddPlayer(Player p)
    {
        _players.Add(p);
        Broadcast(Packet.String(Opcode.S2C_Notify, $"{p.Name} joined map {MapId}"));
    }

    public void RemovePlayer(Player p) => _players.Remove(p);

    private void Broadcast(Packet p)
    {
        foreach (var pl in _players) pl.Session.Send(p);
    }
}