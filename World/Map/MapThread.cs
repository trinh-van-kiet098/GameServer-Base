using GameServer.Core;

namespace GameServer.World.Map;

public sealed class MapThread : GameLoopThread
{
    public GameMap Map { get; }

    public MapThread(int mapId, int tickMs = 50)
        : base($"Map-{mapId}", tickMs)
    {
        Map = new GameMap(mapId);
    }

    public override void Start()
    {
        base.Start();
        Post(() => Map.ScheduleBossSpawn(0));
    }

    protected override void OnTick(long nowMs) => Map.Tick(nowMs);
}