using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using GameServer.Core;

namespace GameServer.Network;

public sealed class SocketListener
{
    private readonly PacketDispatcher _dispatcher;
    private readonly SessionManager _sessions;
    private readonly CancellationTokenSource _cts = new();
    private Socket? _server;

    public SocketListener(PacketDispatcher dispatcher, SessionManager sessions)
    {
        _dispatcher = dispatcher;
        _sessions = sessions;
    }

    public Task StartAsync(int port)
    {
        _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _server.Bind(new IPEndPoint(IPAddress.Any, port));
        _server.Listen(1024);
        Logger.Info($"Listening on :{port}");

        _ = Task.Run(AcceptLoopAsync);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _cts.Cancel();
        try { _server?.Close(); } catch { }
        return Task.CompletedTask;
    }

    private async Task AcceptLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var sock = await _server!.AcceptAsync(_cts.Token);
                sock.NoDelay = true;
                _ = HandleClientAsync(sock);
            }
            catch (Exception ex) when (!_cts.IsCancellationRequested)
            {
                Logger.Error($"accept: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(Socket sock)
    {
        var session = new ClientSession(sock);
        _sessions.Add(session);
        Logger.Info($"client {session.SessionId:N} connected");

        var flushTask = session.FlushLoopAsync(_cts.Token);

        try
        {
            var header = new byte[4];
            while (!_cts.IsCancellationRequested)
            {
                if (!await ReadExactAsync(sock, header, 4, _cts.Token)) break;
                int length = BinaryPrimitives.ReadInt32LittleEndian(header);
                if (length < 2 || length > 64 * 1024) break;

                var body = new byte[length];
                if (!await ReadExactAsync(sock, body, length, _cts.Token)) break;

                ushort opcode = BinaryPrimitives.ReadUInt16LittleEndian(body.AsSpan(0, 2));
                var payload = body.AsSpan(2).ToArray();
                var packet = new Packet(opcode, payload);

                _dispatcher.Dispatch(session, packet);
            }
        }
        catch (Exception ex)
        {
            Logger.Debug($"client {session.SessionId:N} read error: {ex.Message}");
        }
        finally
        {
            session.Close();
            await Task.WhenAny(flushTask, Task.Delay(2000));
            _sessions.Remove(session);
            Logger.Info($"client {session.SessionId:N} disconnected");
        }
    }

    private static async Task<bool> ReadExactAsync(Socket s, byte[] buf, int count, CancellationToken ct)
    {
        int read = 0;
        while (read < count)
        {
            int n = await s.ReceiveAsync(buf.AsMemory(read), SocketFlags.None, ct);
            if (n <= 0) return false;
            read += n;
        }
        return true;
    }
}