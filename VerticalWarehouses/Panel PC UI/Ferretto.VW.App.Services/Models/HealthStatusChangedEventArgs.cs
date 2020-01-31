namespace Ferretto.VW.App.Services
{
    public class HealthStatusChangedEventArgs
    {
        #region Constructors

        public HealthStatusChangedEventArgs(HealthStatus healthStatus, HealthStatus healthWmsStatus)
        {
            this.HealthMasStatus = healthStatus;
            this.HealthWmsStatus = healthWmsStatus;
        }

        #endregion

        #region Properties

        public HealthStatus HealthMasStatus { get; }

        public HealthStatus HealthWmsStatus { get; }

        #endregion
    }
}
