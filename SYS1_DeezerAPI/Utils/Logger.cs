using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Utils
{
    public static class Logger
    {
        private static readonly string _logDirectory =Misc.GetProjectDirectoryPath() + "Logs";
        private static readonly string _logFileName = $"log_{DateTime.Now:dd-MM-yyyy}.txt";

        private static readonly object _consoleLock = new();

        private static StreamWriter _streamWriter;

        static Logger()
        {
            var path = Path.Combine(_logDirectory, _logFileName);

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);

            if (!File.Exists(path))
                File.Create(path).Close();

            _streamWriter = new StreamWriter(path);
        }

        public static void Log(LogLevel level, string message)
        {
            StackFrame frame = new(1);
            MethodBase? method = frame.GetMethod();
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    lock (_streamWriter)
                    {
                        _streamWriter ??= new StreamWriter(Path.Combine(_logDirectory, _logFileName));

                        _streamWriter.WriteLine($"[{DateTime.Now:HH:mm:ss}] {level.ToString().ToUpper()} | {method?.DeclaringType?.FullName ?? ""}.{method?.Name ?? ""}: {message}");
                    }

                    LogConsole(level, $"[{DateTime.Now:HH:mm:ss}] {level.ToString().ToUpper()} | {method?.Name ?? ""}: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logger] Error while trying to log: {ex.Message}");
                }
            });
        }

        private static void LogConsole(LogLevel level, string message)
        {
            lock (_consoleLock)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                switch (level)
                {
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case LogLevel.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.FatalError:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                }

                Console.WriteLine(message);

                Console.ForegroundColor = originalColor;
            }
        }
    }
}
