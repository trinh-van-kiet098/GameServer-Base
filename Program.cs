using GameServer.Core;
using GameServer.Network;
using GameServer.Services;
using GameServer.World.Map;
using GameServer.World.Room;

namespace GameServer;

public static class Program
{
    public static async Task Main()
    {
        Logger.Info("=== GameServer starting ===");

        var db = new DbWorker();
        var tokens = new TokenService();
        var sessions = new SessionManager();
        var auth = new AuthService(tokens, sessions);
        var shop = new ShopService(db);

        var maps = new MapManager();
        var rooms = new RoomManager();

        var dispatcher = new PacketDispatcher(maps, rooms, auth, shop, sessions);

        maps.StartAll(4);

        var listener = new SocketListener(dispatcher, sessions);
        await listener.StartAsync(port: 9000);

        Logger.Info("=== GameServer ready on :9000 ===");
        Logger.Info("Ctrl+C to stop.");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

        try { await Task.Delay(Timeout.Infinite, cts.Token); }
        catch (TaskCanceledException) { }

        Logger.Info("Shutting down...");
        await listener.StopAsync();
        maps.StopAll();
        rooms.StopAll();
        Logger.Info("Goodbye.");
    }
}