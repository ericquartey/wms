using System;

namespace Ferretto.VW.App
{
    public static class NavigationServiceExtensions
    {
        #region Methods

        public static System.Threading.Tasks.Task GoBackSafelyAsync(this Ferretto.VW.App.Services.INavigationService navigationService)
        {
            return System.Windows.Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        navigationService.GoBack();
                    })).Task;
        }

        #endregion
    }
}
