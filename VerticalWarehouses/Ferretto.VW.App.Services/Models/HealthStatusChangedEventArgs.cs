namespace Ferretto.VW.App.Services
{
    public class HealthStatusChangedEventArgs
    {
        #region Constructors

        public HealthStatusChangedEventArgs(HealthStatus healthStatus)
        {
            this.HealthStatus = healthStatus;
        }

        #endregion

        #region Properties

        public HealthStatus HealthStatus { get; }

        #endregion
    }
}
