namespace Ferretto.VW.InstallationApp.Interfaces
{
    public interface INotificationMessageSensorsChangedData : INotificationMessageData
    {
        #region Properties

        bool[] SensorsStates { get; set; }

        #endregion
    }
}
