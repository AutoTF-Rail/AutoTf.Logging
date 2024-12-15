using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AutoTf.Logging;

/// <summary>
/// Logger used in all AutoTF Packages and Demos.
/// Saves data per Default to /var/log/AutoTF/HostName/
/// </summary>
public class Logger : IDisposable
{
    private string _dirPath = Path.Combine("/var/log/AutoTF/", AppDomain.CurrentDomain.FriendlyName);
    private string _filePath = Path.Combine("/var/log/AutoTF/", AppDomain.CurrentDomain.FriendlyName, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

    private readonly ConcurrentQueue<string> _logQueue;
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _cts;
    private Task? _logTask;
    private readonly object _fileLock = new object();

    public event Action<string>? NewLog;

    private bool _isLoggerReady = false;
    private bool _isLinux = true;
    private bool _logToConsole = false;
    
    public Logger(bool logToConsole = false)
    {
        _logToConsole = logToConsole;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _isLinux = false;
        }

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
        if(!_isLinux && !_logToConsole)
            return;
        message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

        if (_logToConsole)
        {
            Console.WriteLine(message);
            return;
        }
        _logQueue.Enqueue(message);
        
        NewLog?.Invoke(message);
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

    private async Task WriteLogToFileAsync(string logEntry)
    {
        if (!_isLoggerReady)
            return;
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
    }

    public void Dispose()
    {
        _cts.Cancel();
        _logTask?.Wait();
        _semaphore.Dispose();
        _cts.Dispose();
    }
}