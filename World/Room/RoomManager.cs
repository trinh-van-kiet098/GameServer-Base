using System.Collections.Concurrent;
using GameServer.Core;

namespace GameServer.World.Room;

public sealed class RoomManager
{
    private readonly ConcurrentDictionary<Guid, RoomThread> _rooms = new();

    public GameRoom CreateRoom(int maxSlots)
    {
        RoomThread? thread = null;

        var room = new GameRoom(maxSlots, onFinished: r =>
        {
            thread?.RequestDestroy();
        });

        thread = new RoomThread(room);
        _rooms[room.RoomId] = thread;
        thread.Start();

        // Watchdog don room khi thread stop
        var t = thread;
        var rid = room.RoomId;
        _ = Task.Run(async () =>
        {
            while (t.IsRunning)
                await Task.Delay(500);

            _rooms.TryRemove(rid, out _);
            Logger.Info($"room {rid:N} removed");
        });

        Logger.Info($"room {room.RoomId:N} created (max={maxSlots})");
        return room;
    }

    public RoomThread? GetThread(Guid roomId)
        => _rooms.GetValueOrDefault(roomId);

    public void StopAll()
    {
        foreach (var t in _rooms.Values) t.Stop();
        _rooms.Clear();
    }
}