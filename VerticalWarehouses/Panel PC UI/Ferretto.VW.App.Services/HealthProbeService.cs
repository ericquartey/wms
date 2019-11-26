using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class HealthProbeService : IHealthProbeService
    {
        #region Fields

        private const int DefaultPollInterval = 3000;

        private const string ErrorMessage = "String cannot be null or empty.";

        private readonly Uri baseAddress;

        private readonly Task healthProbeTask;

        private readonly PubSubEvent<HealthStatusChangedEventArgs> healthStatusChangedEvent;

        private readonly string liveHealthCheckPath;

        private readonly Logger logger;

        private readonly string readyHealthCheckPath;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        private HealthStatus healthStatus;

        private volatile int pollInterval = DefaultPollInterval;

        #endregion

        #region Constructors

        public HealthProbeService(
            Uri baseAddress,
            string liveHealthCheckPath,
            string readyHealthCheckPath,
            IEventAggregator eventAggregator)
        {
            this.baseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
            this.liveHealthCheckPath = liveHealthCheckPath ?? throw new ArgumentNullException(nameof(liveHealthCheckPath));
            this.readyHealthCheckPath = readyHealthCheckPath ?? throw new ArgumentNullException(nameof(readyHealthCheckPath));

            if (eventAggregator is null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            this.healthStatusChangedEvent = eventAggregator.GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>();
            this.logger = NLog.LogManager.GetCurrentClassLogger();

            this.healthProbeTask = new Task(
                async () => await this.RunHealthProbeAsync(this.tokenSource.Token), this.tokenSource.Token);
        }

        #endregion

        #region Properties

        public HealthStatus HealthStatus
        {
            get => this.healthStatus;
            private set
            {
                if (this.healthStatus != value)
                {
                    this.healthStatus = value;

                    this.logger.Debug($"Service at '{this.baseAddress}' is {this.healthStatus}.");

                    this.healthStatusChangedEvent
                        .Publish(new HealthStatusChangedEventArgs(this.healthStatus));
                }
            }
        }

        public PubSubEvent<HealthStatusChangedEventArgs> HealthStatusChanged => this.healthStatusChangedEvent;

        public int PollInterval
        {
            get => this.pollInterval;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The value cannot be negative.");
                }

                this.pollInterval = value;
            }
        }

        #endregion

        #region Methods

        public void Start()
        {
            this.healthProbeTask.Start();
        }

        public void Stop()
        {
            this.tokenSource.Cancel(false);
        }

        private async Task CheckLivelinessStatus(RetryHttpClient client, CancellationToken cancellationToken)
        {
            try
            {
                var livelinessResponse = await this.GetHealthCheckStatus(client, this.liveHealthCheckPath, cancellationToken);
                if (Enum.TryParse<HealthStatus>(livelinessResponse, out var healthStatus))
                {
                    this.HealthStatus = healthStatus;
                }
            }
            catch
            {
                this.HealthStatus = HealthStatus.Unhealthy;
            }
        }

        private async Task CheckReadinessStatus(RetryHttpClient client, CancellationToken cancellationToken)
        {
            try
            {
                var readinessResponseString = await this.GetHealthCheckStatus(client, this.readyHealthCheckPath, cancellationToken);

                if (Enum.TryParse<HealthStatus>(readinessResponseString, out var healthStatus))
                {
                    if (healthStatus == HealthStatus.Degraded || healthStatus == HealthStatus.Unhealthy)
                    {
                        this.HealthStatus = HealthStatus.Initializing;
                    }
                    else if (healthStatus == HealthStatus.Healthy)
                    {
                        this.HealthStatus = HealthStatus.Initialized;
                    }
                }
            }
            catch
            {
                // do nothing
            }
        }

        private async Task<string> GetHealthCheckStatus(RetryHttpClient client, string healthCheckPath, CancellationToken cancellationToken)
        {
            var readinessResponse = await client.SendAsync(
                new System.Net.Http.HttpRequestMessage
                {
                    RequestUri = new Uri(this.baseAddress, healthCheckPath),
                    Method = new System.Net.Http.HttpMethod("GET")
                },
                System.Net.Http.HttpCompletionOption.ResponseContentRead,
                cancellationToken);

            var readinessResponseString = await readinessResponse.Content.ReadAsStringAsync();
            return readinessResponseString;
        }

        private async Task RunHealthProbeAsync(CancellationToken cancellationToken)
        {
            do
            {
                using (var client = new RetryHttpClient { BaseAddress = this.baseAddress })
                {
                    if (this.HealthStatus == HealthStatus.Unknown
                        ||
                        this.HealthStatus == HealthStatus.Unhealthy)
                    {
                        await this.CheckReadinessStatus(client, cancellationToken);
                    }
                    else
                    {
                        await this.CheckLivelinessStatus(client, cancellationToken);
                    }
                }

                await Task.Delay(this.pollInterval);
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}
