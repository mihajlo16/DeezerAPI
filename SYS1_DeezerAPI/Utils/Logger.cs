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
        private static readonly string _logDirectory = Misc.GetProjectDirectoryPath() + "Logs";
        private static readonly string _logFileName = $"log_{DateTime.Now:dd-MM-yyyy}.txt";
        private static readonly object _consoleLock = new();
        private static readonly StreamWriter _streamWriter;
        private static readonly SemaphoreSlim _fileSemaphore = new(1, 1);

        static Logger()
        {
            var path = Path.Combine(_logDirectory, _logFileName);

            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);

            if (!File.Exists(path))
                File.Create(path).Close();

            _streamWriter = new StreamWriter(path, append: true);
        }

        public async static Task Log(LogLevel level, string message)
        {
            try
            {
                await _fileSemaphore.WaitAsync();
                try
                {
                    StringBuilder poruka = new("[");
                    poruka.Append(DateTime.Now.ToString("HH:mm:ss"))
                          .Append("] [")
                          .Append(level.ToString().ToUpper())
                          .Append("] ")
                          .Append(message);

                    await _streamWriter.WriteLineAsync(poruka.ToString());
                    await _streamWriter.FlushAsync();

                    LogConsole(level, poruka.ToString());
                }
                finally
                {
                    _fileSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                lock (_consoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"[Logger] Error while trying to log: {ex.Message}");
                    Console.ForegroundColor = default;
                }
            }
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
