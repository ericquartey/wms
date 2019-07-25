using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface INotificationMessageSensorsChangedData : INotificationMessageData
    {
        #region Properties

        bool[] SensorsStates { get; set; }

        #endregion
    }
}
