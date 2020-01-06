namespace Ferretto.VW.MAS.DataLayer
{
    public interface IWmsSettingsProvider
    {
        #region Properties

        bool IsWmsTimeSyncEnabled { get; set; }

        int TimeSyncIntervalMilliseconds { get; }

        #endregion
    }
}
