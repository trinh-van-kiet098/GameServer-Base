using GameServer.Core;

namespace GameServer.World.Room;

public sealed class RoomThread : GameLoopThread
{
    public GameRoom Room { get; }
    private volatile bool _destroyRequested;

    public RoomThread(GameRoom room, int tickMs = 50)
        : base($"Room-{room.RoomId:N}", tickMs)
    {
        Room = room;
    }

    public void RequestDestroy() => _destroyRequested = true;

    protected override void OnTick(long nowMs)
    {
        Room.Tick(nowMs);

        if (_destroyRequested && Room.State == RoomState.Finished)
            Stop();
    }

    protected override void OnShutdown()
        => Logger.Info($"room thread {Name} shutdown");
}