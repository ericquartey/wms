using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Services;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.Utils;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.VW.App.Services
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IRegionManager regionManager;

        private readonly IRegionNavigationService regionNavigationService;

        private readonly Stack<NavigationTrack> tracks = new Stack<NavigationTrack>();

        #endregion

        #region Constructors

        public NavigationService(
            IUnityContainer unityContainer,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IRegionNavigationService regionNavigationService)
        {
            if (unityContainer == null)
            {
                throw new ArgumentNullException(nameof(unityContainer));
            }

            if (regionManager == null)
            {
                throw new ArgumentNullException(nameof(regionManager));
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (regionNavigationService == null)
            {
                throw new ArgumentNullException(nameof(regionNavigationService));
            }

            this.container = unityContainer;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.regionNavigationService = regionNavigationService;
        }

        #endregion

        #region Methods

        public void Appear(string moduleName, string viewModelName, bool isTrackable, object data = null)
        {
            if (!MvvmNaming.IsViewModelNameValid(viewModelName))
            {
                this.logger.Warn($"Invalid view model name '{viewModelName}' for module name '{moduleName}'.");
                return;
            }

            this.logger.Trace($"Opening view '{viewModelName}' of module '{moduleName}'.");

            try
            {
                this.LoadModule(moduleName);

                var viewName = MvvmNaming.GetViewNameFromViewModelName(viewModelName);

                var parameters = new NavigationParameters();
                parameters.Add(viewModelName, data);

                this.regionManager.RequestNavigate(Utils.Modules.Layout.REGION_MAINCONTENT, viewName, parameters);

                this.tracks.Push(new NavigationTrack(moduleName, viewName, viewModelName, isTrackable));

                this.eventAggregator
                    .GetEvent<NavigationCompleted>()
                    .Publish(new NavigationCompletedPubSubEventArgs(moduleName, viewModelName));
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Cannot show view '{viewModelName}' for module '{moduleName}'.");
            }

            return;
        }

        public void Disappear(INavigableViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            try
            {
                viewModel.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Cannot close view model '{viewModel.GetType().Name}'.");
            }
        }

        public void GoBack()
        {
            if (!this.tracks.Any())
            {
                return;
            }

            NavigationTrack navigationTrack = null;
            do
            {
                navigationTrack = this.tracks.Pop();
            }
            while (!navigationTrack.CanBackTrack && this.tracks.Any());

            this.regionManager.RequestNavigate(
                Utils.Modules.Layout.REGION_MAINCONTENT,
                navigationTrack.ViewName,
                new NavigationParameters());

            this.eventAggregator
                .GetEvent<NavigationCompleted>()
                .Publish(new NavigationCompletedPubSubEventArgs(navigationTrack.ModuleName, navigationTrack.ViewModelName));
        }

        public void LoadModule(string moduleName)
        {
            var catalog = this.container.Resolve<IModuleCatalog>();
            var module = catalog.Modules.FirstOrDefault(m => m.ModuleName == moduleName);
            if (module == null)
            {
                this.logger.Error($"Module {moduleName} not found.");
                return;
            }

            if (module.State != ModuleState.NotStarted)
            {
                return;
            }

            var moduleManager = this.container.Resolve<IModuleManager>();
            moduleManager.LoadModule(moduleName);
        }

        public void SetBusy(bool isBusy)
        {
            if (Application.Current.MainWindow.Descendants<View>().FirstOrDefault() is View view &&
                view.DataContext is IBusyViewModel busyViewModel)
            {
                busyViewModel.IsBusy = isBusy;
            }
        }

        public object SubscribeToNavigationCompleted(Action<NavigationCompletedPubSubEventArgs> action)
        {
            return this.eventAggregator
                .GetEvent<NavigationCompleted>()
                .Subscribe(action);
        }

        public void UnsubscribeToNavigationCompleted(object subscriptionToken)
        {
            if (subscriptionToken is SubscriptionToken token)
            {
                this.eventAggregator
                    .GetEvent<NavigationCompleted>()
                    .Unsubscribe(token);
            }
        }

        #endregion
    }
}
