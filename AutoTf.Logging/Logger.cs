using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AutoTf.Logging;

/// <summary>
/// Logger used in all AutoTF Packages and Demos.
/// Saves data per Default to /var/log/AutoTF/HostName/Date
/// Main support lays on Linux distros, Windows is supported too, due to some packages needing the logger.
/// In the case of windows, you can turn on Logging to console in the ctor.
/// </summary>
public class Logger : IDisposable
{
    private readonly string _dirPath = Path.Combine("/var/log/AutoTF/", AppDomain.CurrentDomain.FriendlyName);
    private readonly string _filePath = Path.Combine("/var/log/AutoTF/", AppDomain.CurrentDomain.FriendlyName, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

    private readonly ConcurrentQueue<string> _logQueue;
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _cts;
    
    private readonly bool _isLoggerReady;
    private readonly bool _isLinux;
    private readonly bool _logToConsole;
    
    private readonly object _fileLock = new object();
    
    private Task? _logTask;
    
    public event Action<string>? NewLog;
    
    public Logger(bool logToConsole = false)
    {
        _logToConsole = logToConsole;
        
        _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        _logQueue = new ConcurrentQueue<string>();
        _semaphore = new SemaphoreSlim(1, 1);
        _cts = new CancellationTokenSource();

        if (_isLinux)
        {
            StartLogging();
            Directory.CreateDirectory(_dirPath);
        }

        _isLoggerReady = true;
    }

    public void Log(string message)
    {
        if (!_isLoggerReady)
            return;
        
        // If the current platform is not linux, and logToConsole is disabled, we can return because we don't need to log anything.
        if(!_isLinux && !_logToConsole)
            return;
        
        message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

        // If it is linux, AND logToConsole is enabled, we only need to log to console and not to the file.
        Console.WriteLine(message);
        NewLog?.Invoke(message);
        _logQueue.Enqueue(message);
    }

    private void StartLogging()
    {
        // ReSharper disable once AsyncVoidLambda
        _logTask = Task.Run(async void () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await ProcessLogsAsync();
                await Task.Delay(100);
            }
        });
    }
    
    private async Task ProcessLogsAsync()
    {
        if (_logQueue.IsEmpty)
            return;

        await _semaphore.WaitAsync();
        try
        {
            while (_logQueue.TryDequeue(out var logEntry))
            {
                await WriteLogToFileAsync(logEntry);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private Task WriteLogToFileAsync(string logEntry)
    {
        if (!_isLoggerReady)
            return Task.CompletedTask;
        try
        {
            lock (_fileLock)
            {
                File.AppendAllText(_filePath, logEntry + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing log: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _logTask?.Wait();
        _semaphore.Dispose();
        _cts.Dispose();
    }
}