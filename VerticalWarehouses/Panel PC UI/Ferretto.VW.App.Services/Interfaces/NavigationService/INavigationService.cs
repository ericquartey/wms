﻿namespace Ferretto.VW.App.Services
{
    public interface INavigationService
    {
        #region Properties

        bool IsBusy { get; set; }

        #endregion

        #region Methods

        void Appear(string moduleName, string viewModelName, object data = null, bool trackCurrentView = true);

        void Disappear(INavigableViewModel viewModel);

        void GoBack();

        void GoBackTo(string modelName, string viewModelName);

        void LoadModule(string moduleName);

        object SubscribeToNavigationCompleted(System.Action<NavigationCompletedPubSubEventArgs> action);

        void UnsubscribeToNavigationCompleted(object subscriptionToken);

        #endregion
    }
}
