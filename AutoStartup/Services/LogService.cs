using NLog;
using NLog.Config;
using NLog.Targets;
using System.IO;
using System.Text;

namespace AutoStartup.Services
{
    public static class LogService
    {
        public static void RegisterServiceLogger(string serviceName)
        {
            var config = LogManager.Configuration ?? new LoggingConfiguration();

            string logDirectory = Path.Combine(Environment.CurrentDirectory, $"Logs/{serviceName}");

            var fileTarget = new FileTarget(serviceName)
            {
                FileName = Path.Combine(logDirectory, $"{serviceName}.log"),
                Layout = "[${longdate}][${uppercase:${level}}] ${message}${exception:format=tostring}",
                ArchiveFileName = $"{logDirectory}/Archives/{serviceName}",
                ArchiveAboveSize = 10 * 1024 * 1024,
                ArchiveSuffixFormat = "_{1:yyyyMMdd_HHmmss}.log",
                CreateDirs = true,
                KeepFileOpen = true,
                Encoding = Encoding.UTF8,
            };

            config.AddTarget(fileTarget);

            var rule = new LoggingRule($"{serviceName}", LogLevel.Debug, LogLevel.Fatal, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        public static LinkedList<string> ReadLastNLineLog(string serviceName, int lines, string pattern)
        {
            string logDirectory = Path.Combine(Environment.CurrentDirectory, $"Logs/{serviceName}");
            string path = Path.Combine(logDirectory, $"{serviceName}.log");
            if (!File.Exists(path))
            {
                return [];
            }
            LinkedList<string> nLogs = [];
            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new(fileStream, Encoding.UTF8);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine() ?? string.Empty;
                if (line.Contains(pattern))
                {
                    line = line.Replace(pattern, string.Empty);
                    nLogs.AddLast(line);
                    if (nLogs.Count > lines)
                    {
                        nLogs.RemoveFirst();
                    }
                }
            }

            return nLogs;
        }
    }
}