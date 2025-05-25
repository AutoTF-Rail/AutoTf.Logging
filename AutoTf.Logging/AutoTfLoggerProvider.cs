using Microsoft.Extensions.Logging;

namespace AutoTf.Logging;

public class AutoTfLoggerProvider : ILoggerProvider
{
    private readonly Logger _logger;

    public AutoTfLoggerProvider(Logger logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new AutoTfLogger(_logger, categoryName);
    }

    public void Dispose()
    {
        _logger.Dispose();
    }
}