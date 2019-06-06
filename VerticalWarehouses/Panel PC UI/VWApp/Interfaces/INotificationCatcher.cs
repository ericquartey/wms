namespace Ferretto.VW.VWApp.Interfaces
{
    public interface INotificationCatcher
    {
        #region Methods

        void SubscribeInstallationMethodsToMAService();

        void SubscribeOperatorMethodsToMAService();

        #endregion
    }
}
