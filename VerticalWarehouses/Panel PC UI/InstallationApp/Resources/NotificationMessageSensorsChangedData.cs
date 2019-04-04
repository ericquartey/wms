using Ferretto.VW.InstallationApp.Interfaces;

namespace Ferretto.VW.InstallationApp.Resources
{
    public class NotificationMessageSensorsChangedData : INotificationMessageSensorsChangedData
    {
        #region Constructors

        public NotificationMessageSensorsChangedData(bool[] sensorsStates)
        {
            this.SensorsStates = sensorsStates;
        }

        #endregion

        #region Properties

        public decimal? CurrentPosition { get; set; }

        public bool[] SensorsStates { get; set; }

        #endregion
    }
}
