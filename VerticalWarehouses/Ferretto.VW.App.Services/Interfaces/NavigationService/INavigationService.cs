namespace Ferretto.VW.App.Services.Interfaces
{
    public interface INavigationService
    {
        #region Methods

        void Appear(string moduleName, string viewModelName, bool keepTrack, object data = null);

        void Disappear(INavigableViewModel viewModel);

        void GoBack();

        void LoadModule(string moduleName);

        void SetBusy(bool isBusy);

        object SubscribeToNavigationCompleted(System.Action<NavigationCompletedPubSubEventArgs> action);

        void UnsubscribeToNavigationCompleted(object subscriptionToken);

        #endregion
    }
}
