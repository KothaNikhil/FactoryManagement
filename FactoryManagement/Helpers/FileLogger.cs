using System;
using System.IO;

namespace FactoryManagement.Helpers
{
    public static class FileLogger
    {
        private static readonly string LogDirectory;
        private static readonly object _lockObject = new object();

        static FileLogger()
        {
            // Create logs directory in AppData\Local\FactoryManagement\Logs
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            LogDirectory = Path.Combine(appDataPath, "FactoryManagement", "Logs");
            
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create log directory: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lockObject)
                {
                    var logFile = Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logMessage = $"{timestamp} - {message}";
                    
                    File.AppendAllText(logFile, logMessage + Environment.NewLine);
                    System.Diagnostics.Debug.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FileLogger error: {ex.Message}");
            }
        }

        public static string GetLogsDirectory()
        {
            return LogDirectory;
        }
    }
}
