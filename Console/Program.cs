using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Logging.ClearProviders();
builder.Logging.AddCustomLogger(config =>
{
    config.MinLogLevel = LogLevel.Information;
});

using IHost host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogDebug(1, "Does this line get hit?");    
logger.LogInformation("Nothing to see here."); 
logger.LogWarning(5, "Warning... that was odd."); 
logger.LogError(7, "Oops, there was an error.");  
logger.LogTrace(5, "== 120.");                    

await host.RunAsync();

public class CustomLoggerConfiguration
{
    public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
}

public class CustomLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Func<CustomLoggerConfiguration> _getCurrentConfig;

    public CustomLogger(string categoryName, Func<CustomLoggerConfiguration> getCurrentConfig)
    {
        _categoryName = categoryName;
        _getCurrentConfig = getCurrentConfig;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= _getCurrentConfig().MinLogLevel;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}] [{_categoryName}] {formatter(state, exception)}");
    }
}

public class CustomLoggerProvider : ILoggerProvider
{
    private readonly CustomLoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();

    public CustomLoggerProvider(Func<CustomLoggerConfiguration> getCurrentConfig)
    {
        _currentConfig = getCurrentConfig();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new CustomLogger(name, GetCurrentConfig));
    }

    private CustomLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public static class CustomLoggerExtensions
{
    public static ILoggingBuilder AddCustomLogger(
        this ILoggingBuilder builder,
        Action<CustomLoggerConfiguration> configure)
    {
        builder.Services.AddSingleton<Func<CustomLoggerConfiguration>>(() =>
        {
            var config = new CustomLoggerConfiguration();
            configure(config);
            return config;
        });

        builder.Services.AddSingleton<ILoggerProvider, CustomLoggerProvider>();

        return builder;
    }
}