using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Core
{
    internal class MasHealthCheckStep : Step
    {
        #region Constructors

        public MasHealthCheckStep(int number, string title, string description, int timeout, string log, bool skipOnResume)
            : base(number, title, description, log, skipOnResume)
        {
            this.Timeout = timeout;
        }

        #endregion

        #region Properties

        public int Timeout { get; }

        public string Url { get; set; }

        #endregion

        #region Methods

        protected override async Task<StepStatus> OnApplyAsync()
        {
            using (var httpClient = new HttpClient())
            {
                string status = null;
                var requestUri = new Uri(InterpolateVariables(this.Url));
                var startTime = DateTime.Now;
                var isHealthy = false;
                do
                {
                    try
                    {
                        this.LogInformation($"HTTP GET {requestUri}");
                        var message = await httpClient.GetAsync(requestUri);
                        status = await message.Content.ReadAsStringAsync();

                        this.LogInformation($"HTTP response: '{status}'");
                    }
                    catch
                    {
                        this.LogInformation("HTTP request failed.");
                    }

                    isHealthy = status?.Equals("healthy", StringComparison.OrdinalIgnoreCase) == true;

                    await Task.Delay(1000);
                }
                while (!isHealthy && (DateTime.Now - startTime).TotalMilliseconds < this.Timeout);

                this.LogInformation(isHealthy
                    ? "Service is healthy."
                    : $"Service was not healthy after {this.Timeout}ms.");

                return isHealthy
                    ? StepStatus.Done
                    : StepStatus.Failed;
            }
        }

        protected override Task<StepStatus> OnRollbackAsync()
        {
            this.LogInformation("Nulla da annullare in questo step.");

            return Task.FromResult(StepStatus.RolledBack);
        }

        #endregion
    }
}
