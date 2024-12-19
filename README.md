# AutoTF.Logging

A really really simple Logger that is used in several AutoTF Applications/Packages.

Logs to "/var/log/AutoTF/HostName/Date"

It's mainly based on Linux systems, but can run on Windows too without writing to any files.

To use it on windows, and still see logs from packages, set the "logToConsole" bool to true in the Logger ctor.

### Using the logger
To use the logger, you just have to initialize it, and use it using it's Log method.
```csharp
private Logger _logger = new Logger(logToConsole: false);
_logger.Log("Hello world");
```

#### Redirecting logs
To redirect logs and use them yourself too, you can hook yourself onto the NewLog event.
```csharp
_logger.NewLog += NewLog;

private void NewLog(string obj)
{
    Console.WriteLine(obj);
}
```


## Support and Contributions
If you would like to contribute to this package, or get support for it, feel free to contact us at [opensource@autotf.de](opensource@autotf.de)