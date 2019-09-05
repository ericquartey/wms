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

        private readonly IModuleManager moduleManager;

        private readonly IRegionManager regionManager;

        private readonly IRegionNavigationService regionNavigationService;

        private readonly Stack<NavigationTrack> tracks = new Stack<NavigationTrack>();

        #endregion

        #region Constructors

        public NavigationService(
            IUnityContainer unityContainer,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IRegionNavigationService regionNavigationService,
            IModuleManager moduleManager)
        {
            if (unityContainer is null)
            {
                throw new ArgumentNullException(nameof(unityContainer));
            }

            if (regionManager is null)
            {
                throw new ArgumentNullException(nameof(regionManager));
            }

            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (regionNavigationService is null)
            {
                throw new ArgumentNullException(nameof(regionNavigationService));
            }

            if (moduleManager is null)
            {
                throw new ArgumentNullException(nameof(moduleManager));
            }

            this.container = unityContainer;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.regionNavigationService = regionNavigationService;
            this.moduleManager = moduleManager;
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get
            {
                var viewModel = this.GetBusyViewModel();
                if (viewModel is null)
                {
                    return false;
                }

                return viewModel.IsBusy;
            }
            set
            {
                var viewModel = this.GetBusyViewModel();
                if (viewModel != null)
                {
                    viewModel.IsBusy = value;
                }
            }
        }

        #endregion

        #region Methods

        public void Appear(string moduleName, string viewModelName, object data = null, bool trackCurrentView = true)
        {
            if (!MvvmNaming.IsViewModelNameValid(viewModelName))
            {
                this.logger.Warn($"Unable to navigate to view '{moduleName}.{viewModelName}' because name is invalid.");
                return;
            }

            this.logger.Trace($"Navigating to view '{moduleName}.{viewModelName}'.");

            try
            {
                this.LoadModule(moduleName);

                var viewName = MvvmNaming.GetViewNameFromViewModelName(viewModelName);

                var parameters = new NavigationParameters();
                parameters.Add(viewModelName, data);

                this.DisappearActiveView();

                this.regionManager.RequestNavigate(Utils.Modules.Layout.REGION_MAINCONTENT, viewName, parameters);

                if (trackCurrentView)
                {
                    var currentViewRecord = this.tracks.Peek();
                    this.logger.Warn($"Marking view '{currentViewRecord.ModuleName}.{currentViewRecord.ViewModelName}' as trackable.");
                    currentViewRecord.IsTrackable = true;
                }

                this.tracks.Push(new NavigationTrack(moduleName, viewName, viewModelName));

                this.eventAggregator
                    .GetEvent<NavigationCompleted>()
                    .Publish(new NavigationCompletedPubSubEventArgs(moduleName, viewModelName));

                this.ClearNotifications();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Cannot show view '{moduleName}.{viewModelName}'.");
            }
        }

        public void Disappear(INavigableViewModel viewModel)
        {
            if (viewModel is null)
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
                this.logger.Warn($"Unable to navigate back because navigation stack is empty.");
                return;
            }

            var currentHistoryRecord = this.tracks.Pop();
            this.logger.Trace($"Navigating back from '{currentHistoryRecord.ModuleName}.{currentHistoryRecord.ViewName}' ...");

            while (this.tracks.Any()
                &&
                !this.tracks.Peek().IsTrackable)
            {
                var currentRecord = this.tracks.Peek();
                this.logger.Trace($"Discarding history view '{currentRecord.ModuleName}.{currentRecord.ViewName}' because not marked as trackable.");
                this.tracks.Pop();
            }

            this.NavigateBackTo(this.tracks.Peek());
        }

        public void GoBackTo(string modelName, string viewModelName)
        {
            if (!this.tracks.Any(t => t.ModuleName == modelName && t.ViewModelName == viewModelName))
            {
                this.logger.Warn($"Unable to navigate back to '{modelName}.{viewModelName}' because no view with the specified name was found in the navigation stack.");
                return;
            }

            if (this.tracks.Peek().ModuleName == modelName
                &&
                this.tracks.Peek().ViewModelName == viewModelName)
            {
                this.logger.Info($"Back navigation to '{modelName}.{viewModelName}' not performed because it matches the current view");
                return;
            }

            var currentHistoryRecord = this.tracks.Pop();
            this.logger.Trace($"Navigating back from '{currentHistoryRecord.ModuleName}.{currentHistoryRecord.ViewName}' ...");

            while (this.tracks.Any()
                &&
                this.tracks.Peek().ModuleName != modelName
                &&
                this.tracks.Peek().ViewModelName != viewModelName)
            {
                this.tracks.Pop();
            }

            this.ClearNotifications();

            this.NavigateBackTo(this.tracks.Peek());
        }

        public void LoadModule(string moduleName)
        {
            var catalog = this.container.Resolve<IModuleCatalog>();
            var module = catalog.Modules.FirstOrDefault(m => m.ModuleName == moduleName);
            if (module is null)
            {
                this.logger.Error($"Module '{moduleName}': unable to load the module bacause it is not present in the catalog.");
                return;
            }

            if (module.State == ModuleState.NotStarted)
            {
                this.logger.Debug($"Module '{moduleName}': loading module ...");

                this.moduleManager.LoadModule(moduleName);

                this.logger.Debug($"Module '{moduleName}': loaded.");
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

        private void ClearNotifications()
        {
            this.eventAggregator
              .GetEvent<PresentationNotificationPubSubEvent>()
              .Publish(new PresentationNotificationMessage(true));
        }

        private void DisappearActiveView()
        {
            var activeView = this.regionManager.Regions[Utils.Modules.Layout.REGION_MAINCONTENT].ActiveViews.FirstOrDefault();

            if (activeView is View view && view.DataContext is ViewModelBase viewModel)
            {
                viewModel.Disappear();
            }
        }

        private IBusyViewModel GetBusyViewModel()
        {
            if (Application.Current.MainWindow.Descendants<View>().FirstOrDefault() is View view &&
                view.DataContext is IBusyViewModel busyViewModel)
            {
                return busyViewModel;
            }

            return null;
        }

        private void NavigateBackTo(NavigationTrack historyRecord)
        {
            this.DisappearActiveView();

            this.logger.Debug($"Navigating back to '{historyRecord.ModuleName}.{historyRecord.ViewName}'.");

            this.regionManager.RequestNavigate(
                Utils.Modules.Layout.REGION_MAINCONTENT,
                historyRecord.ViewName,
                new NavigationParameters());

            this.eventAggregator
                .GetEvent<NavigationCompleted>()
                .Publish(new NavigationCompletedPubSubEventArgs(historyRecord.ModuleName, historyRecord.ViewModelName));
        }

        #endregion
    }
}
