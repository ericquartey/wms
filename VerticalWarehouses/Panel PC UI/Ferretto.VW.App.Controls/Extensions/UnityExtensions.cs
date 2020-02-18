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
            if (navigationOptions is null)
            {
                throw new System.ArgumentNullException(nameof(navigationOptions));
            }

            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IDialogService, DialogService>();
            containerRegistry.RegisterSingleton<IWmsImagesProvider, WmsImagesProvider>();

            var navigationService = containerRegistry.GetContainer().Resolve<NavigationService>();
            navigationService.MainContentRegionName = navigationOptions.MainContentRegionName;
            containerRegistry.RegisterInstance<INavigationService>(navigationService);

            return containerRegistry;
        }

        #endregion
    }
}
