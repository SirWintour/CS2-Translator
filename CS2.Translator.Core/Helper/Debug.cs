namespace CS2.Translator.Core.Helper
{
    public static class DebugLogger
    {
        private static readonly object _lock = new();
        private static string? _logFilePath;
        public static bool Enabled { get; private set; }

        public static void Initialize(bool enableDebug)
        {
            Enabled = enableDebug;
            if (!Enabled)
                return;

            try
            {
                string baseDir;

                if (OperatingSystem.IsWindows())
                {
                    baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                }
                else
                {
                    baseDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".config"
                    );
                }

                string appDir = Path.Combine(baseDir, "CS2-Translator");
                string logDir = Path.Combine(appDir, "logs");
                Directory.CreateDirectory(logDir);

                _logFilePath = Path.Combine(logDir, "debug.log");

                File.WriteAllText(_logFilePath, $"[Debug Start] {DateTime.Now}\n");
                Log($"Logger initialized on {GetPlatformName()} at {_logFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DebugLogger] Initialization failed: {ex.Message}");
            }
        }


        public static void Log(string message)
        {
            string formatted = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            
            Console.WriteLine(formatted);

            if (!Enabled || _logFilePath is null)
                return;

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, formatted + Environment.NewLine);
                }
            }
            catch
            {
                // Ignore write errors
            }
        }

        public static void LogException(Exception ex, string? context = null)
        {
            string msg = $"[EXCEPTION] {context ?? "Unhandled"}: {ex.Message}\n{ex.StackTrace}";
            Log(msg);
        }

        public static void RotateIfNeeded(long maxBytes = 5_000_000)
        {
            if (!Enabled || _logFilePath is null)
                return;

            try
            {
                var info = new FileInfo(_logFilePath);
                if (info.Exists && info.Length > maxBytes)
                {
                    string archive = Path.Combine(
                        info.DirectoryName!,
                        $"debug_{DateTime.Now:yyyyMMdd_HHmmss}.log"
                    );

                    File.Move(_logFilePath, archive, overwrite: false);
                    File.WriteAllText(_logFilePath, $"[Debug Rotated] {DateTime.Now}\n");
                    Log($"Log rotated - {archive}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DebugLogger] Rotation failed: {ex.Message}");
            }
        }
        
        public static void OpenLogFolder()
        {
            if (_logFilePath is null)
                return;

            try
            {
                string? folder = Path.GetDirectoryName(_logFilePath);
                if (folder is null)
                    return;

                if (OperatingSystem.IsWindows())
                {
                    System.Diagnostics.Process.Start("explorer.exe", folder);
                }
                else if (OperatingSystem.IsLinux())
                {
                    System.Diagnostics.Process.Start("xdg-open", folder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DebugLogger] Could not open folder: {ex.Message}");
            }
        }

        public static string GetPlatformName() =>
            OperatingSystem.IsWindows() ? "Windows" :
            OperatingSystem.IsLinux() ? "Linux" :
            "Unknown";
    }
}