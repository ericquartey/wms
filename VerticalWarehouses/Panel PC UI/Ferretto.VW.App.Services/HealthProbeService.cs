using System;
using System.Net.Http;
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

        private readonly Uri baseMasAddress;

        private readonly Task healthProbeMasTask;

        private readonly Task healthProbeWmsTask;

        private readonly PubSubEvent<HealthStatusChangedEventArgs> healthStatusChangedEvent;

        private readonly string liveHealthCheckPath;

        private readonly Logger logger;

        private readonly string readyHealthCheckPath;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        private readonly IMachineWmsStatusWebService wmsStatusWebService;

        private HealthStatus healthMasStatus;

        private HealthStatus healthWmsStatus;

        private volatile int pollInterval = DefaultPollInterval;

        #endregion

        #region Constructors

        public HealthProbeService(
            Uri baseMasAddress,
            string liveHealthCheckPath,
            string readyHealthCheckPath,
            IMachineWmsStatusWebService wmsStatusWebService,
            IEventAggregator eventAggregator)
        {
            this.baseMasAddress = baseMasAddress ?? throw new ArgumentNullException(nameof(baseMasAddress));
            this.liveHealthCheckPath = liveHealthCheckPath ?? throw new ArgumentNullException(nameof(liveHealthCheckPath));
            this.readyHealthCheckPath = readyHealthCheckPath ?? throw new ArgumentNullException(nameof(readyHealthCheckPath));
            this.wmsStatusWebService = wmsStatusWebService ?? throw new ArgumentNullException(nameof(wmsStatusWebService));

            if (eventAggregator is null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            this.healthStatusChangedEvent = eventAggregator.GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>();
            this.logger = NLog.LogManager.GetCurrentClassLogger();

            this.healthProbeMasTask = new Task(
                async () => await this.RunHealthProbeMasAsync(this.tokenSource.Token), this.tokenSource.Token);

            this.healthProbeWmsTask = new Task(
                async () => await this.RunHealthProbeWmsAsync(this.tokenSource.Token), this.tokenSource.Token);
        }

        #endregion

        #region Properties

        public HealthStatus HealthMasStatus
        {
            get => this.healthMasStatus;
            private set
            {
                if (this.healthMasStatus != value)
                {
                    this.healthMasStatus = value;

                    this.logger.Debug($"Service at '{this.baseMasAddress}' is {this.healthMasStatus}.");

                    this.healthStatusChangedEvent
                        .Publish(new HealthStatusChangedEventArgs(this.healthMasStatus, this.healthWmsStatus));
                }
            }
        }

        public PubSubEvent<HealthStatusChangedEventArgs> HealthStatusChanged => this.healthStatusChangedEvent;

        public HealthStatus HealthWmsStatus
        {
            get => this.healthWmsStatus;
            private set
            {
                if (this.healthWmsStatus != value)
                {
                    this.healthWmsStatus = value;

                    this.logger.Debug($"WMS service is {this.healthWmsStatus}.");

                    this.healthStatusChangedEvent
                        .Publish(new HealthStatusChangedEventArgs(this.healthMasStatus, this.healthWmsStatus));
                }
            }
        }

        public int PollInterval
        {
            get => this.pollInterval;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Resources.Localized.Get("ServiceHealthProbe.ValueCannotNegative"));
                }

                this.pollInterval = value;
            }
        }

        #endregion

        #region Methods

        public void Start()
        {
            this.healthProbeMasTask.Start();
            this.healthProbeWmsTask?.Start();
        }

        public void Stop()
        {
            this.tokenSource.Cancel(false);
        }

        private async Task<HealthStatus> CheckLivelinessStatus(HttpClient client, CancellationToken cancellationToken)
        {
            try
            {
                var livelinessResponse = await this.GetHealthCheckStatus(client, this.liveHealthCheckPath, cancellationToken);
                if (Enum.TryParse<HealthStatus>(livelinessResponse, out var healthStatus))
                {
                    return healthStatus;
                }
            }
            catch
            {
                return HealthStatus.Unhealthy;
            }

            return HealthStatus.Unknown;
        }

        private async Task<HealthStatus> CheckReadinessStatus(HttpClient client, CancellationToken cancellationToken)
        {
            this.logger.Debug($"Checking readiness of service at '{client.BaseAddress}' ...");

            try
            {
                var readinessResponseString = await this.GetHealthCheckStatus(client, this.readyHealthCheckPath, cancellationToken);

                if (Enum.TryParse<HealthStatus>(readinessResponseString, out var healthStatus))
                {
                    if (healthStatus == HealthStatus.Degraded || healthStatus == HealthStatus.Unhealthy)
                    {
                        return HealthStatus.Initializing;
                    }
                    else if (healthStatus == HealthStatus.Healthy)
                    {
                        return HealthStatus.Initialized;
                    }
                }
            }
            catch
            {
                // do nothing
            }

            return HealthStatus.Unknown;
        }

        private async Task<string> GetHealthCheckStatus(HttpClient client, string healthCheckPath, CancellationToken cancellationToken)
        {
            using (var message = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress, healthCheckPath),
                Method = new HttpMethod(HttpMethod.Get.Method)
            })
            {
                var response = await client.SendAsync(message,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken);

                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
        }

        private async Task RunHealthProbeMasAsync(CancellationToken cancellationToken)
        {
            do
            {
                using (var client = new HttpClient { BaseAddress = this.baseMasAddress })
                {
                    if (this.HealthMasStatus == HealthStatus.Unknown
                        ||
                        this.HealthMasStatus == HealthStatus.Unhealthy
                        ||
                        this.HealthMasStatus == HealthStatus.Initializing)
                    {
                        this.HealthMasStatus = await this.CheckReadinessStatus(client, cancellationToken);
                    }
                    else
                    {
                        this.HealthMasStatus = await this.CheckLivelinessStatus(client, cancellationToken);
                    }
                }

                await Task.Delay(this.pollInterval);
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        private async Task RunHealthProbeWmsAsync(CancellationToken cancellationToken)
        {
            do
            {
                try
                {
                    var healthStatusString = await this.wmsStatusWebService.GetHealthAsync();

                    if (Enum.TryParse<HealthStatus>(healthStatusString, out var healthStatus))
                    {
                        this.HealthWmsStatus = healthStatus;
                    }
                    else
                    {
                        this.logger.Debug($"Unable to parse health status of WMS (response was '{healthStatus}').");

                        this.HealthWmsStatus = HealthStatus.Unknown;
                    }
                }
                catch
                {
                    this.HealthWmsStatus = HealthStatus.Unhealthy;
                }

                await Task.Delay(this.pollInterval);
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}
