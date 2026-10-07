using System.Threading.Channels;
using GameServer.Core;

namespace GameServer.Services;

public sealed class DbWorker
{
    private readonly Channel<Func<Task>> _queue =
        Channel.CreateUnbounded<Func<Task>>();

    public DbWorker() { _ = Task.Run(ProcessLoop); }

    public void Enqueue(Func<Task> job) => _queue.Writer.TryWrite(job);

    private async Task ProcessLoop()
    {
        await foreach (var job in _queue.Reader.ReadAllAsync())
        {
            try { await job(); }
            catch (Exception ex) { Logger.Error($"db: {ex}"); }
        }
    }
}