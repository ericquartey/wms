namespace Ferretto.VW.MAS.TimeManagement
{
    public interface ISystemTimeProvider
    {
        #region Properties

        bool CanEnableWmsAutoSyncMode { get; }

        bool IsWmsAutoSyncEnabled { get; set; }

        #endregion

        #region Methods

        void SetSystemTime(System.DateTime dateTime);

        #endregion
    }
}
