namespace GameServer.Core;

public sealed class MinHeapTimer
{
    private readonly SortedSet<TimerEntry> _set = new();

    private sealed class TimerEntry : IComparable<TimerEntry>
    {
        public long DueMs;
        public long Seq;
        public Action Callback = null!;

        public int CompareTo(TimerEntry? other)
        {
            if (other is null) return 1;
            int c = DueMs.CompareTo(other.DueMs);
            return c != 0 ? c : Seq.CompareTo(other.Seq);
        }
    }

    private long _seq;

    public void Schedule(long nowMs, int delayMs, Action callback)
    {
        _set.Add(new TimerEntry
        {
            DueMs = nowMs + delayMs,
            Seq = _seq++,
            Callback = callback
        });
    }

    public void Tick(long nowMs)
    {
        while (_set.Count > 0)
        {
            var first = _set.Min!;
            if (first.DueMs > nowMs) break;
            _set.Remove(first);
            try { first.Callback(); }
            catch (Exception ex) { Logger.Error($"timer: {ex}"); }
        }
    }
}