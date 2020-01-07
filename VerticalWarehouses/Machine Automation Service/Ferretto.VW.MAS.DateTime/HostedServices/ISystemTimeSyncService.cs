namespace Ferretto.VW.MAS.TimeManagement
{
    public interface ISystemTimeSyncService
    {
        #region Methods

        void Disable();

        void Enable();

        void SetSystemTime(System.DateTime dateTime);

        #endregion
    }
}
