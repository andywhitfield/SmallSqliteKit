using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallSqliteKit.Service.Data;

namespace SmallSqliteKit.Service.Jobs
{
    public abstract class BackgroundServiceJob : BackgroundService
    {
        private static int _initialDelay = 0;
        private static readonly SemaphoreSlim _runJobLock = new(1);

        private readonly IServiceProvider _serviceProvider;
        protected readonly ILogger<BackgroundServiceJob> _logger;

        public BackgroundServiceJob(IServiceProvider serviceProvider, ILogger<BackgroundServiceJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Running {GetType().Name} job service");
            try
            {
                _initialDelay += 1000;
                await Task.Delay(_initialDelay, stoppingToken);
                do
                {
                    _logger.LogDebug("Waiting for run lock...");
                    await _runJobLock.WaitAsync(stoppingToken);
                    _logger.LogDebug("Got run lock, starting job");

                    TimeSpan timeUntilDue;
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var configRepository = scope.ServiceProvider.GetRequiredService<IConfigRepository>();
                        (var runInterval, var lastRunDateTime) = await GetRunIntervalsAsync(configRepository);

                        var timeWhenRunDue = lastRunDateTime + runInterval;
                        _logger.LogTrace($"Interval: {runInterval}; last run: {lastRunDateTime}; due: {timeWhenRunDue}");
                        var now = DateTime.UtcNow;
                        if (timeWhenRunDue <= now)
                        {
                            _logger.LogInformation($"Running {GetType().Name} job");
                            try
                            {
                                await RunJobAsync(scope);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error running job: {GetType().Name}");
                            }
                            await configRepository.AddOrUpdateAsync(LastRunDateTimeConfigName, DateTime.UtcNow.ToString(DateExtensions.DefaultDateTimeFormat));
                            timeUntilDue = runInterval;
                        }
                        else
                        {
                            timeUntilDue = timeWhenRunDue - now;
                        }
                    }
                    finally
                    {
                        _runJobLock.Release();
                    }

                    _logger.LogInformation($"{GetType().Name} job service - waiting [{timeUntilDue}] before running again");
                    await Task.Delay(timeUntilDue, stoppingToken);
                } while (!stoppingToken.IsCancellationRequested);
            }
            catch (TaskCanceledException)
            {
                _logger.LogDebug($"{GetType().Name} job background service cancellation token cancelled - service stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred running {GetType().Name} job background service");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopped {GetType().Name} job background service");
            return Task.CompletedTask;
        }

        protected abstract Task RunJobAsync(IServiceScope serviceScope);

        private async Task<(TimeSpan RunInterval, DateTime LastRunDateTime)> GetRunIntervalsAsync(IConfigRepository configRepository)
        {
            var allSettings = await configRepository.GetAllAsync();
            return (allSettings.FirstOrDefault(c => c.ConfigName == RunIntervalConfigName)?.ConfigValue.ToTimeSpan() ?? TimeSpan.FromHours(4),
                allSettings.FirstOrDefault(s => s.ConfigName == LastRunDateTimeConfigName)?.ConfigValue.ToDateTime() ?? DateTime.MinValue);
        }

        private string RunIntervalConfigName => $"{GetType().Name}.RunInterval";
        private string LastRunDateTimeConfigName => $"{GetType().Name}.LastRunDateTime";
    }
}