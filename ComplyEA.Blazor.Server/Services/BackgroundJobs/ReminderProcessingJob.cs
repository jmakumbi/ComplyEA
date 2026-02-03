using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Data.Filtering;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.Services;

namespace ComplyEA.Blazor.Server.Services.BackgroundJobs
{
    /// <summary>
    /// Background service that processes due reminders at regular intervals.
    /// </summary>
    public class ReminderProcessingJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderProcessingJob> _logger;
        private TimeSpan _interval = TimeSpan.FromMinutes(15);

        // Configuration keys
        private const string KEY_PROCESSING_ENABLED = "Reminders.Processing.Enabled";
        private const string KEY_INTERVAL_MINUTES = "Reminders.Processing.IntervalMinutes";
        private const string KEY_MAX_RETRIES = "Reminders.Processing.MaxRetries";

        public ReminderProcessingJob(
            IServiceProvider serviceProvider,
            ILogger<ReminderProcessingJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Processing Job started.");

            // Initial delay to allow application to fully start
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected during shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reminders.");
                }

                // Reload interval from configuration
                _interval = await GetProcessingIntervalAsync();

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Reminder Processing Job stopped.");
        }

        private async Task ProcessRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                // Get XPO data store provider
                var dataStoreAccessor = scope.ServiceProvider.GetService<XpoDataStoreProviderAccessor>();
                if (dataStoreAccessor?.DataStoreProvider == null)
                {
                    _logger.LogWarning("XPO data store provider not available. Skipping reminder processing.");
                    return;
                }

                // Create ObjectSpace for this processing run
                var objectSpaceProvider = new XPObjectSpaceProvider(dataStoreAccessor.DataStoreProvider, true);
                using var objectSpace = objectSpaceProvider.CreateObjectSpace();

                // Check if processing is enabled
                if (!IsProcessingEnabled(objectSpace))
                {
                    _logger.LogDebug("Reminder processing is disabled in configuration.");
                    return;
                }

                var maxRetries = GetMaxRetries(objectSpace);

                // Create services
                var emailService = new SmtpEmailService(objectSpace);
                var notificationService = new NotificationService(emailService);

                _logger.LogInformation("Processing due reminders...");

                // Process due reminders
                var result = await notificationService.ProcessDueRemindersAsync(objectSpace);

                if (result.Processed > 0)
                {
                    objectSpace.CommitChanges();
                    _logger.LogInformation(
                        "Processed {Processed} reminders: {Sent} sent, {Failed} failed.",
                        result.Processed, result.Sent, result.Failed);
                }
                else
                {
                    _logger.LogDebug("No due reminders to process.");
                }

                // Retry failed reminders
                if (result.Failed > 0 || await HasFailedRemindersAsync(objectSpace, maxRetries))
                {
                    var retryCount = await notificationService.RetryFailedRemindersAsync(objectSpace, maxRetries);
                    if (retryCount > 0)
                    {
                        objectSpace.CommitChanges();
                        _logger.LogInformation("Retried {Count} failed reminders.", retryCount);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reminder processing.");
                throw;
            }
        }

        private bool IsProcessingEnabled(IObjectSpace objectSpace)
        {
            var config = objectSpace.FindObject<SystemConfiguration>(
                new BinaryOperator("Key", KEY_PROCESSING_ENABLED));
            return config?.GetBoolValue(true) ?? true;
        }

        private int GetMaxRetries(IObjectSpace objectSpace)
        {
            var config = objectSpace.FindObject<SystemConfiguration>(
                new BinaryOperator("Key", KEY_MAX_RETRIES));
            return config?.GetIntValue(3) ?? 3;
        }

        private async Task<TimeSpan> GetProcessingIntervalAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dataStoreAccessor = scope.ServiceProvider.GetService<XpoDataStoreProviderAccessor>();
                if (dataStoreAccessor?.DataStoreProvider == null)
                    return _interval;

                var objectSpaceProvider = new XPObjectSpaceProvider(dataStoreAccessor.DataStoreProvider, true);
                using var objectSpace = objectSpaceProvider.CreateObjectSpace();

                var config = objectSpace.FindObject<SystemConfiguration>(
                    new BinaryOperator("Key", KEY_INTERVAL_MINUTES));
                var minutes = config?.GetIntValue(15) ?? 15;

                return TimeSpan.FromMinutes(Math.Max(1, minutes)); // Minimum 1 minute
            }
            catch
            {
                return _interval;
            }
        }

        private Task<bool> HasFailedRemindersAsync(IObjectSpace objectSpace, int maxRetries)
        {
            var count = (int)objectSpace.Evaluate(
                typeof(ComplyEA.Module.BusinessObjects.Compliance.ComplianceReminder),
                CriteriaOperator.Parse("Count()"),
                CriteriaOperator.And(
                    new BinaryOperator("DeliveryStatus.Code", "FAILED"),
                    new BinaryOperator("RetryCount", maxRetries, BinaryOperatorType.Less)
                ));

            return Task.FromResult(count > 0);
        }
    }
}
