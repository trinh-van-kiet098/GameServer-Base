using System.Diagnostics;

namespace GameServer.Core;

public abstract class GameLoopThread
{
    private readonly Thread _thread;
    private readonly Mailbox _mailbox = new();
    private readonly int _tickMs;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private volatile bool _running;

    protected GameLoopThread(string name, int tickMs = 50)
    {
        _tickMs = tickMs;
        _thread = new Thread(Run)
        {
            Name = name,
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal
        };
    }

    public string Name => _thread.Name ?? "loop";
    public long NowMs => _clock.ElapsedMilliseconds;
    public bool IsRunning => _running;

    public virtual void Start()
    {
        _running = true;
        _thread.Start();
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _mailbox.Complete();
        _thread.Join(2000);
    }

    public void Post(Action action) => _mailbox.Post(action);

    public Task<T> PostAsync<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(() =>
        {
            try { tcs.SetResult(func()); }
            catch (Exception ex) { tcs.SetException(ex); }
        });
        return tcs.Task;
    }

    private void Run()
    {
        long nextTick = 0;
        while (_running)
        {
            long now = _clock.ElapsedMilliseconds;

            if (now >= nextTick)
            {
                try { OnTick(now); }
                catch (Exception ex) { Logger.Error($"[{Name}] tick: {ex}"); }
                nextTick = now + _tickMs;
            }

            int budget = 1000;
            while (budget-- > 0 && _mailbox.TryRead(out var action))
            {
                try { action(); }
                catch (Exception ex) { Logger.Error($"[{Name}] mailbox: {ex}"); }
            }

            Thread.Sleep(1);
        }

        try { OnShutdown(); }
        catch (Exception ex) { Logger.Error($"[{Name}] shutdown: {ex}"); }
    }

    protected abstract void OnTick(long nowMs);
    protected virtual void OnShutdown() { }
}