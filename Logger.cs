using System;
using System.Diagnostics;
using System.IO;

public static class Logger
{
    private static readonly string _filePath =
        Path.Combine(AppContext.BaseDirectory, "errorlog.txt");

    private static readonly object _lock = new object();

    public static void Log(string message)
    {
        try
        {
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

            lock (_lock)
            {
                File.AppendAllText(_filePath, logLine);
            }
        }
        catch
        {
            Debug.WriteLine("Failed to write to log file.");
        }
    }

    public static void LogError(string message, Exception ex)
    {
        Log($"{message} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}");
    }
}
