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

        public string BaseUrl => ConfigurationManager.AppSettings.Get("MAS:BaseUrl");

        public int Timeout { get; }

        #endregion

        #region Methods

        protected override async Task<StepStatus> OnApplyAsync()
        {
            using (var httpClient = new HttpClient())
            {
                string status = null;
                var requestUri = new Uri($"{this.BaseUrl}/health/live");
                var startTime = DateTime.Now;
                do
                {
                    try
                    {
                        var message = await httpClient.GetAsync(requestUri);
                        status = await message.Content.ReadAsStringAsync();
                    }
                    catch
                    {
                        // do nothing
                    }
                }
                while (!status.Equals("healthy", StringComparison.OrdinalIgnoreCase) && (startTime - DateTime.Now).TotalMilliseconds < this.Timeout);

                return status.Equals("healthy", StringComparison.OrdinalIgnoreCase)
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
