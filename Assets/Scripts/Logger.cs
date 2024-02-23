using System;
using System.IO;

public static class Logger
{
    private static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GameLogs.log");

    public static void Log(string message)
    {
        using (StreamWriter sw = File.AppendText(logFilePath))
        {
            sw.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
