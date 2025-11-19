using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BookingManagementService.Common.AppConfigurations.App.interfaces;
using BookingManagementService.BusinessLogic.Helpers.DateHelpers;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using BookingManagementService.Common.Configurations.Consul.interfaces;
using BookingManagementService.Infrastructure.Models.Consul.DistributedLock;
using BookingManagementService.Infrastructure.Services.Consul.DistributedLock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BookingManagementService.BusinessLogic.Services.DbServices.BookingServices;

namespace BookingManagementService.BusinessLogic.Services.BackgroundServices.BookingJobs
{
    public class BookingTrackPreviewingResponseTimeoutJob : BackgroundService
    {
        private readonly ConsulDistributedLockService _lockService;
        private readonly IBackgroundJobsConfig _jobsConfig;
        private readonly ILogger<BookingTrackPreviewingResponseTimeoutJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsulDistributedLockConfig _consulDistributedLockConfig;
        private readonly IAppConfig _appConfig;
        private readonly DateHelper _dateHelper;

        // Job configuration
        private BackgroundJob _jobConfig => _jobsConfig.BookingTrackPreviewingResponseTimeoutJob;
        private CronExpression? _cronExpression;

        public BookingTrackPreviewingResponseTimeoutJob(
            ConsulDistributedLockService lockService,
            IBackgroundJobsConfig jobsConfig,
            ILogger<BookingTrackPreviewingResponseTimeoutJob> logger,
            IServiceProvider serviceProvider,
            IAppConfig appConfig,
            DateHelper dateHelper,
            IConsulDistributedLockConfig consulDistributedLockConfig)
        {
            _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
            _jobsConfig = jobsConfig ?? throw new ArgumentNullException(nameof(jobsConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _dateHelper = dateHelper ?? throw new ArgumentNullException(nameof(dateHelper));
            _consulDistributedLockConfig = consulDistributedLockConfig ?? throw new ArgumentNullException(nameof(consulDistributedLockConfig));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Validate and parse cron expression
            try
            {
                _cronExpression = CronExpression.Parse(
                    _jobConfig.CronExpression,
                    CronFormat.IncludeSeconds); // Support seconds format

                _logger.LogInformation(
                    "Background job starting: {JobName}, Enabled={IsEnabled}, Cron={Cron}, LockKey={LockKey}",
                    nameof(BookingTrackPreviewingResponseTimeoutJob),
                    _jobConfig.IsEnabled,
                    _jobConfig.CronExpression,
                    _jobConfig.ConsulLockKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid cron expression: {CronExpression}",
                    _jobConfig.CronExpression);
                throw;
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_jobConfig.IsEnabled)
            {
                _logger.LogInformation("Background job is disabled: {JobName}", nameof(BookingTrackPreviewingResponseTimeoutJob));
                return;
            }

            _logger.LogInformation(
                "Background job started: {JobName}, Description={Description}",
                nameof(BookingTrackPreviewingResponseTimeoutJob),
                _jobConfig.Description);

            await WaitForNextRoundTimeAsync(stoppingToken);

            // Main execution loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var next = _cronExpression!.GetNextOccurrence(now, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE));

                    if (!next.HasValue)
                    {
                        _logger.LogWarning("No next occurrence found for cron expression: {Cron}", _jobConfig.CronExpression);
                        break;
                    }

                    var delay = next.Value - now;
                    if (delay > TimeSpan.Zero)
                    {
                        _logger.LogTrace(
                            "Waiting {Delay}ms until next execution at {NextRun}",
                            delay.TotalMilliseconds,
                            next.Value);

                        await Task.Delay(delay, stoppingToken);
                    }

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await ExecuteJobWithLockAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Background job cancelled: {JobName}", nameof(BookingTrackPreviewingResponseTimeoutJob));
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in background job execution loop: {JobName}", nameof(BookingTrackPreviewingResponseTimeoutJob));
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Background job stopped: {JobName}", nameof(BookingTrackPreviewingResponseTimeoutJob));
        }


        /// <summary>
        /// Execute the job with distributed lock to prevent multiple instances from running
        /// </summary>
        private async Task ExecuteJobWithLockAsync(CancellationToken cancellationToken)
        {
            var executionId = Guid.NewGuid().ToString("N")[..8];
            var startTime = _dateHelper.GetNowByAppTimeZone();

            try
            {
                _logger.LogDebug(
                    "Attempting to acquire lock: ExecutionId={ExecutionId}, LockKey={LockKey}",
                    executionId,
                    _jobConfig.ConsulLockKey);

                // Prepare lock options
                var lockOptions = new ConsulLockOptions
                {
                    TTLSeconds = _jobConfig.ConsulLockTTLSeconds,
                    RenewalIntervalSeconds = _jobConfig.ConsulLockRenewalIntervalSeconds,
                    // WaitTimeout = TimeSpan.FromSeconds(2), // Don't wait long - another instance is running
                    WaitTimeout = _consulDistributedLockConfig.DefaultLockWaitTimeoutSeconds > 0
                        ? TimeSpan.FromSeconds(_consulDistributedLockConfig.DefaultLockWaitTimeoutSeconds)
                        : TimeSpan.FromSeconds(30),
                    // RetryDelay = TimeSpan.FromMilliseconds(500),
                    RetryDelay = _consulDistributedLockConfig.DefaultLockRetryDelaySeconds > 0
                        ? TimeSpan.FromSeconds(_consulDistributedLockConfig.DefaultLockRetryDelaySeconds)
                        : TimeSpan.FromMilliseconds(1000),
                    Metadata = new Dictionary<string, string>
                    {
                        ["JobName"] = nameof(BookingTrackPreviewingResponseTimeoutJob),
                        ["ExecutionId"] = executionId,
                        ["InstanceId"] = Environment.MachineName,
                        ["ProcessId"] = Environment.ProcessId.ToString(),
                        ["ScheduledAt"] = startTime.ToString("O")
                    }
                };

                // Execute job with automatic lock management
                await _lockService.ExecuteWithLockAsync(
                    _jobConfig.ConsulLockKey,
                    async ct =>
                    {
                        _logger.LogInformation(
                            "Job execution started: ExecutionId={ExecutionId}, LockAcquired=true",
                            executionId);

                        // Execute the actual job logic
                        await ExecuteJobLogicAsync(executionId, ct);

                        var duration = (_dateHelper.GetNowByAppTimeZone() - startTime).TotalMilliseconds;
                        _logger.LogInformation(
                            "Job execution completed: ExecutionId={ExecutionId}, Duration={Duration}ms",
                            executionId,
                            duration);
                    },
                    lockOptions,
                    cancellationToken);
            }
            catch (LockAcquisitionException)
            {
                // Another instance is running - this is NORMAL and EXPECTED
                _logger.LogDebug(
                    "Lock held by another instance, skipping execution: ExecutionId={ExecutionId}",
                    executionId);
            }
            catch (SessionExpiredException ex)
            {
                // Session expired during job execution - job ran too long
                _logger.LogError(
                    ex,
                    "Session expired during job execution: ExecutionId={ExecutionId}, Duration={Duration}ms. " +
                    "Consider increasing ConsulLockTTLSeconds in configuration.",
                    executionId,
                    (_dateHelper.GetNowByAppTimeZone() - startTime).TotalMilliseconds);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "Job execution cancelled: ExecutionId={ExecutionId}",
                    executionId);
                throw; // Re-throw to stop the service
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Job execution failed: ExecutionId={ExecutionId}, Duration={Duration}ms",
                    executionId,
                    (_dateHelper.GetNowByAppTimeZone() - startTime).TotalMilliseconds);

                // Don't re-throw - continue to next scheduled execution
            }
        }

        /// <summary>
        /// The actual job logic
        /// </summary>
        private async Task ExecuteJobLogicAsync(string executionId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting booking track previewing response timeout: ExecutionId={ExecutionId}", executionId);


                // Example: Use scoped services
                using (var scope = _serviceProvider.CreateScope())
                {
                    var bookingService = scope.ServiceProvider.GetRequiredService<BookingService>();
                    await bookingService.BookingTrackPreviewingResponseTimeoutAsync();
                }

                Console.WriteLine("\n ----Booking track previewing response timeout----");
                // in thời điểm hiện tại
                Console.WriteLine($"✅ Current time: {_dateHelper.GetNowByAppTimeZone()} \n");


                _logger.LogDebug("Booking track previewing response timeout successfully: ExecutionId={ExecutionId}", executionId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Job logic cancelled: ExecutionId={ExecutionId}", executionId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating Booking track previewing response timeout: ExecutionId={ExecutionId}",
                    executionId);
                throw;
            }
        }

        private async Task WaitForNextRoundTimeAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE);
            var nextRoundTime = _cronExpression!.GetNextOccurrence(now, timezone);

            if (!nextRoundTime.HasValue)
            {
                _logger.LogWarning("Cannot calculate next round time, starting immediately");
                return;
            }

            var waitDuration = nextRoundTime.Value - now;

            if (waitDuration > TimeSpan.Zero)
            {
                _logger.LogInformation(
                    "Waiting until first round time: NextRun={NextRun:HH:mm:ss.fff} (in {WaitSeconds:F1}s)",
                    nextRoundTime.Value,
                    waitDuration.TotalSeconds);

                await Task.Delay(waitDuration, cancellationToken);

                _logger.LogInformation(
                    "Reached first round time: {RoundTime:HH:mm:ss.fff}, starting job loop",
                    DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation("Already at round time, starting immediately");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Background job stopping: {JobName}",
                nameof(BookingTrackPreviewingResponseTimeoutJob));

            await base.StopAsync(cancellationToken);

            _logger.LogInformation(
                "Background job stopped: {JobName}",
                nameof(BookingTrackPreviewingResponseTimeoutJob));
        }
    }
}
