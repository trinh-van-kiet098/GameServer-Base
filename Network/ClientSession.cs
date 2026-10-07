using System.Net.Sockets;
using System.Threading.Channels;
using GameServer.Core;
using GameServer.World.Entity;

namespace GameServer.Network;

public sealed class ClientSession
{
    public Guid SessionId { get; } = Guid.NewGuid();
    public long AccountId { get; set; }
    public int CurrentMapId { get; set; }
    public Guid? CurrentRoomId { get; set; }
    public Player? Player { get; set; }

    private readonly Channel<Packet> _outbound =
        Channel.CreateUnbounded<Packet>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    private readonly Socket _socket;

    public ClientSession(Socket socket) { _socket = socket; }

    public void Send(Packet p) => _outbound.Writer.TryWrite(p);

    public async Task FlushLoopAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var p in _outbound.Reader.ReadAllAsync(ct))
            {
                var bytes = p.Serialize();
                var sent = 0;
                while (sent < bytes.Length)
                {
                    int n = await _socket.SendAsync(bytes.AsMemory(sent), SocketFlags.None, ct);
                    if (n <= 0) return;
                    sent += n;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.Debug($"session {SessionId:N} closed: {ex.Message}");
        }
        finally
        {
            _outbound.Writer.TryComplete();
            try { _socket.Close(); } catch { }
        }
    }

    public void Close() => _outbound.Writer.TryComplete();
}