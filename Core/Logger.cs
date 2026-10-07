namespace GameServer.Core;

public static class Logger
{
    private static readonly object _lock = new();

    public static void Info(string msg)  => Write("INFO ", msg, ConsoleColor.Gray);
    public static void Warn(string msg)  => Write("WARN ", msg, ConsoleColor.Yellow);
    public static void Error(string msg) => Write("ERROR", msg, ConsoleColor.Red);
    public static void Debug(string msg) => Write("DEBUG", msg, ConsoleColor.DarkGray);

    private static void Write(string level, string msg, ConsoleColor color)
    {
        lock (_lock)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{level}] [{Environment.CurrentManagedThreadId:D2}] {msg}");
            Console.ForegroundColor = prev;
        }
    }
}