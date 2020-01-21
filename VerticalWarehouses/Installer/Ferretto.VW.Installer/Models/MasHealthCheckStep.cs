using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer
{
    internal class MasHealthCheckStep : Step
    {
        #region Constructors

        public MasHealthCheckStep(int number, string title, int timeout)
            : base(number, title)
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
                var requestUri = new Uri(this.InterpolateVariables(this.Url));
                var startTime = DateTime.Now;
                var isHealthy = false;
                do
                {
                    try
                    {
                        this.LogWriteLine($"HTTP GET {requestUri}");
                        var message = await httpClient.GetAsync(requestUri);
                        status = await message.Content.ReadAsStringAsync();

                        this.LogWriteLine($"HTTP response: '{status}'");
                    }
                    catch
                    {
                        this.LogWriteLine("HTTP request failed.");
                    }

                    isHealthy = status?.Equals("healthy", StringComparison.OrdinalIgnoreCase) == true;
                }
                while (!isHealthy && (DateTime.Now - startTime).TotalMilliseconds < this.Timeout);

                this.LogWriteLine(isHealthy
                    ? "Service is healthy."
                    : $"Service was not healthy after {this.Timeout}ms.");

                return isHealthy
                    ? StepStatus.Done
                    : StepStatus.Failed;
            }
        }

        protected override Task<StepStatus> OnRollbackAsync()
        {
            return Task.FromResult(StepStatus.RolledBack);
        }

        #endregion
    }
}
