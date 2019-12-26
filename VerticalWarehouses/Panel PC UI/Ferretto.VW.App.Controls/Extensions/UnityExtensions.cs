using Ferretto.VW.App.Controls.Models;
using Ferretto.VW.App.Services;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using Unity;

namespace Ferretto.VW.App.Controls
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry RegisterUiControlServices(
            this IContainerRegistry containerRegistry,
            NavigationOptions navigationOptions)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IDialogService, DialogService>();

            var navigationService = containerRegistry.GetContainer().Resolve<NavigationService>();
            navigationService.MainContentRegionName = navigationOptions.MainContentRegionName;
            containerRegistry.RegisterInstance<INavigationService>(navigationService);

            return containerRegistry;
        }

        #endregion
    }
}
