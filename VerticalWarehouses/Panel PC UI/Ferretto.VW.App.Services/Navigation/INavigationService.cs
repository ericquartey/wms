namespace Ferretto.VW.App.Services
{
    public interface INavigationService
    {
        #region Properties

        bool IsBusy { get; set; }

        #endregion

        #region Methods

        void Appear(string moduleName, string viewModelName, object data = null, bool trackCurrentView = true);

        void Disappear(INavigableViewModel viewModel);

        INavigableView GetActiveView();

        INavigableViewModel GetActiveViewModel();

        void GoBack();

        void GoBackTo(string modelName, string viewModelName, string caller);

        bool IsActiveView(string moduleName, string viewModelName);

        void LoadModule(string moduleName);

        object SubscribeToNavigationCompleted(System.Action<NavigationCompletedEventArgs> action);

        byte[] TakeScreenshot(bool checkWithPrevious);

        void UnsubscribeToNavigationCompleted(object subscriptionToken);

        #endregion
    }
}
