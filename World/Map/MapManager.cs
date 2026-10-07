using GameServer.Core;

namespace GameServer.World.Map;

public sealed class MapManager
{
    private readonly Dictionary<int, MapThread> _maps = new();

    public void StartAll(int count = 4)
    {
        for (int i = 1; i <= count; i++)
        {
            var t = new MapThread(i);
            _maps[i] = t;
            t.Start();
        }
        Logger.Info($"started {count} map threads");
    }

    public MapThread Get(int mapId) => _maps[mapId];

    public void StopAll()
    {
        foreach (var m in _maps.Values) m.Stop();
        _maps.Clear();
    }
}