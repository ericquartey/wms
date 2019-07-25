using Ferretto.VW.App.Installation.Interfaces;

namespace Ferretto.VW.App.Installation.Resources
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

        public bool[] SensorsStates { get; set; }

        #endregion
    }
}
