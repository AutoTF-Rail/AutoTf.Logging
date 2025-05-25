using Microsoft.Extensions.Logging;

namespace AutoTf.Logging;

public class AutoTfLogger : ILogger
{
    private readonly Logger _customLogger;
    private readonly string _category;

    public AutoTfLogger(Logger logger, string category)
    {
        _customLogger = logger;
        _category = category;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        string message = formatter(state, exception);

        string logMessage = $"[{logLevel}] {_category} - {message}";

        if (exception != null)
            logMessage += $" Exception: {exception}";

        _customLogger.Log(logMessage, includeCaller: false);
    }
}