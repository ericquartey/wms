namespace Ferretto.VW.App.Services
{
    public interface ITimeSyncService
    {
        #region Properties

        int SyncIntervalMilliseconds { get; set; }

        #endregion

        #region Methods

        void Start();

        void Stop();

        #endregion
    }
}
