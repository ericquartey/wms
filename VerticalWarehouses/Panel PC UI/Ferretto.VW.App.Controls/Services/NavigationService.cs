using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ferretto.VW.App.Controls.Services;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils;
using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.VW.App.Controls
{
    internal class NavigationService : INavigationService
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IModuleManager moduleManager;

        private readonly Stack<NavigationHistoryRecord> navigationStack = new Stack<NavigationHistoryRecord>();

        private readonly IRegionManager regionManager;

        #endregion

        #region Constructors

        public NavigationService(
            IUnityContainer unityContainer,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IModuleManager moduleManager)
        {
            this.container = unityContainer ?? throw new ArgumentNullException(nameof(unityContainer));
            this.regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.moduleManager = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));
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

        internal string MainContentRegionName { get; set; }

        #endregion

        #region Methods

        public void Appear(string moduleName, string viewModelName, object data = null, bool trackCurrentView = true)
        {
            if (!MvvmNaming.IsViewModelNameValid(viewModelName))
            {
                this.logger.Warn($"Unable to navigate to view '{moduleName}.{viewModelName}' because its name is invalid.");
                return;
            }

            var activeViewModel = this.GetActiveViewModel();
            if (activeViewModel?.GetType()?.Name == viewModelName)
            {
                this.logger.Warn($"Requested view '{moduleName}.{viewModelName}' is already active.");
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
                this.regionManager.RequestNavigate(this.MainContentRegionName, viewName, parameters);

                if (this.navigationStack.Count > 0)
                {
                    var currentViewRecord = this.navigationStack.Peek();
                    this.logger.Trace($"Marking view '{currentViewRecord.ModuleName}.{currentViewRecord.ViewModelName}' as trackable.");
                    currentViewRecord.IsTrackable = trackCurrentView;

                    this.navigationStack.Where(s => s.ModuleName == moduleName
                                             &&
                                             s.ViewName == viewName
                                             &&
                                             s.IsTrackable).All(ns => ns.IsTrackable = false);
                }

                this.navigationStack.Push(new NavigationHistoryRecord(moduleName, viewName, viewModelName));

                this.eventAggregator
                    .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                    .Publish(new NavigationCompletedEventArgs(moduleName, viewModelName));
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

        public INavigableView GetActiveView()
        {
            var activeView = this.regionManager.Regions[this.MainContentRegionName].ActiveViews.FirstOrDefault();

            if (activeView is View view)
            {
                return view;
            }

            return null;
        }

        public INavigableViewModel GetActiveViewModel()
        {
            var activeView = this.regionManager.Regions[this.MainContentRegionName].ActiveViews.FirstOrDefault();

            if (activeView is View view && view.DataContext is ViewModelBase viewModel)
            {
                return viewModel;
            }

            return null;
        }

        public void GoBack()
        {
            // allow cancelation
            var context = new BackNavigationContext();
            this.GetActiveViewModel().OnNavigatingBack(context);
            if (context.Cancel)
            {
                return;
            }

            if (!this.navigationStack.Any())
            {
                this.logger.Warn($"Unable to navigate back because navigation stack is empty.");
                return;
            }

            var currentHistoryRecord = this.navigationStack.Pop();
            this.logger.Trace($"Navigating back from '{currentHistoryRecord.ModuleName}.{currentHistoryRecord.ViewName}' ...");

            while (this.navigationStack.Any()
                &&
                !this.navigationStack.Peek().IsTrackable)
            {
                var currentRecord = this.navigationStack.Peek();
                this.logger.Trace($"Discarding history view '{currentRecord.ModuleName}.{currentRecord.ViewName}' because not marked as trackable.");
                this.navigationStack.Pop();
            }

            if (this.navigationStack.Any())
            {
                this.NavigateBackTo(this.navigationStack.Peek());
            }
        }

        public void GoBackTo(string moduleName, string viewModelName)
        {
            if (!this.navigationStack.Any(t => t.ModuleName == moduleName && t.ViewModelName == viewModelName))
            {
                this.logger.Warn($"Unable to navigate back to '{moduleName}.{viewModelName}' because no view with the specified name was found in the navigation stack.");

                this.GoBack();
                this.Appear(moduleName, viewModelName);

                return;
            }

            if (this.navigationStack.Peek().ModuleName == moduleName
                &&
                this.navigationStack.Peek().ViewModelName == viewModelName)
            {
                this.logger.Info($"Back navigation to '{moduleName}.{viewModelName}' not performed because it matches the current view");
                return;
            }

            var currentHistoryRecord = this.navigationStack.Pop();
            this.logger.Trace($"Navigating back from '{currentHistoryRecord.ModuleName}.{currentHistoryRecord.ViewName}' ...");

            while (this.navigationStack.Any()
                &&
                this.navigationStack.Peek().ModuleName != moduleName
                &&
                this.navigationStack.Peek().ViewModelName != viewModelName)
            {
                this.navigationStack.Pop();
            }

            this.ClearNotifications();

            if (this.navigationStack.Any())
            {
                this.NavigateBackTo(this.navigationStack.Peek());
            }
        }

        public bool IsActiveView(string moduleName, string viewModelName)
        {
            return this.GetActiveViewModel().GetType().Name == viewModelName;
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

        public object SubscribeToNavigationCompleted(Action<NavigationCompletedEventArgs> action)
        {
            return this.eventAggregator
                .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                .Subscribe(action);
        }

        public void UnsubscribeToNavigationCompleted(object subscriptionToken)
        {
            if (subscriptionToken is SubscriptionToken token)
            {
                this.eventAggregator
                    .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
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
            this.GetActiveViewModel()?.Disappear();
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

        private void NavigateBackTo(NavigationHistoryRecord historyRecord)
        {
            this.logger.Debug($"Navigating back to '{historyRecord.ModuleName}.{historyRecord.ViewName}'.");

            this.DisappearActiveView();

            this.regionManager.RequestNavigate(
                this.MainContentRegionName,
                historyRecord.ViewName,
                new NavigationParameters());

            this.eventAggregator
                .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                .Publish(new NavigationCompletedEventArgs(historyRecord.ModuleName, historyRecord.ViewModelName));
        }

        #endregion
    }
}
