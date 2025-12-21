namespace FamilyFinance.Services;

public class LogService
{
    private readonly string _logPath;
    private readonly ILogger<LogService> _logger;
    private static readonly object _lock = new();

    public LogService(ILogger<LogService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _logPath = Path.Combine(env.ContentRootPath, "logs");
        
        if (!Directory.Exists(_logPath))
        {
            Directory.CreateDirectory(_logPath);
        }
    }

    public void LogInfo(string message, string? context = null)
    {
        Log("INFO", message, context);
    }

    public void LogWarning(string message, string? context = null)
    {
        Log("WARN", message, context);
    }

    public void LogError(string message, Exception? ex = null, string? context = null)
    {
        var fullMessage = ex != null 
            ? $"{message}\n  Exception: {ex.GetType().Name}: {ex.Message}\n  StackTrace: {ex.StackTrace}"
            : message;
        
        Log("ERROR", fullMessage, context);
    }

    public void LogImport(string message, bool isError = false)
    {
        Log(isError ? "IMPORT-ERR" : "IMPORT", message, "Import");
    }

    private void Log(string level, string message, string? context = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var contextStr = context != null ? $"[{context}] " : "";
        var logLine = $"[{timestamp}] [{level}] {contextStr}{message}";

        // Console log
        switch (level)
        {
            case "ERROR":
            case "IMPORT-ERR":
                _logger.LogError(logLine);
                break;
            case "WARN":
                _logger.LogWarning(logLine);
                break;
            default:
                _logger.LogInformation(logLine);
                break;
        }

        // File log
        var fileName = $"familyfinance_{DateTime.Now:yyyyMMdd}.log";
        var filePath = Path.Combine(_logPath, fileName);

        lock (_lock)
        {
            try
            {
                File.AppendAllText(filePath, logLine + Environment.NewLine);
            }
            catch
            {
                // Silently fail if we can't write to log file
            }
        }
    }

    public List<string> GetRecentLogs(int count = 100)
    {
        var logs = new List<string>();
        var today = $"familyfinance_{DateTime.Now:yyyyMMdd}.log";
        var filePath = Path.Combine(_logPath, today);

        if (File.Exists(filePath))
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                logs = lines.TakeLast(count).Reverse().ToList();
            }
            catch
            {
                // Ignore read errors
            }
        }

        return logs;
    }

    public string GetLogFilePath()
    {
        return Path.Combine(_logPath, $"familyfinance_{DateTime.Now:yyyyMMdd}.log");
    }
}


