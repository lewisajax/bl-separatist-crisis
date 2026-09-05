using System;
using System.IO;

namespace SeparatistCrisis.BountyHunting
{
    /// <summary>
    /// Static file logger that writes timestamped messages to a log file under the
    /// game's ModLogs directory. Never throws.
    /// </summary>
    public static class BountyLogger
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Mount and Blade II Bannerlord",
            "Configs",
            "ModLogs",
            "BountyHunting.log");

        private static readonly object LockObj = new object();

        /// <summary>
        /// Appends a timestamped line to the log file, creating the directory if
        /// needed. Any failure is silently swallowed.
        /// </summary>
        public static void Log(string message)
        {
            try
            {
                lock (LockObj)
                {
                    string directory = Path.GetDirectoryName(LogFilePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                    File.AppendAllText(LogFilePath, line + Environment.NewLine);
                }
            }
            catch
            {
            }
        }
    }
}
