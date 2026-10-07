using System.Threading.Channels;

namespace GameServer.Core;

public sealed class Mailbox
{
    private readonly Channel<Action> _channel =
        Channel.CreateUnbounded<Action>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

    public void Post(Action action) => _channel.Writer.TryWrite(action);

    public bool TryRead(out Action action) => _channel.Reader.TryRead(out action!);

    public void Complete() => _channel.Writer.TryComplete();
}