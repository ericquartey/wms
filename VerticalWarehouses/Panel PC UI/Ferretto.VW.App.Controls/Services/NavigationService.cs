using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.WindowsUI;
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

        private byte[] previousScreenshot;

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
                this.logger.Warn($"Appear| Unable to navigate to view '{moduleName}.{viewModelName}' because its name is invalid.");
                return;
            }

            // Scratch variable used to force the appearance of view
            var activeViewForceToAppear = false;

            var activeViewModel = this.GetActiveViewModel();
            if (activeViewModel?.GetType()?.Name == viewModelName)
            {
                //this.logger.Warn($"Requested view '{moduleName}.{viewModelName}' is already active.");
                //return;

                //
                // This code has been introduced to handling the behavior of loading unit view for a internal double bay vs the other configuration's bay.
                // For other configuration's bay (like BES, BIG, BIS):
                //  - the view is not forced to be displayed
                // For internal double bay:
                //  - the active view disappears, and it is forced to re-appear in order to display the correct information of loading unit
                //    the view is of Operator.ItemOperations.LOADING_UNIT type (mandatory)
                if (activeViewModel?.GetType()?.Name == Ferretto.VW.Utils.Modules.Operator.ItemOperations.LOADING_UNIT)
                {
                    this.logger.Trace($"Appear| ActiveViewModel={activeViewModel?.GetType()?.Name} :: Requested viewNodelName={viewModelName}, NavigationStack count={this.navigationStack.Count}");
                    // Retrieve the view from the navigation stack
                    if (this.navigationStack.Count > 0)
                    {
                        var currentViewRecord = this.navigationStack.Peek();

                        // Current view must be referred a loading unit and data argument is not null (it is referred to a loading unit)
                        if (currentViewRecord.Id != null && data != null)
                        {
                            var id = currentViewRecord.Id;
                            var loadingUnitId = ((int?)data).Value;

                            // Check if current view are referred to the same loading unit
                            if (id == loadingUnitId)
                            {
                                this.logger.Warn($"Appear| Requested view '{moduleName}.{viewModelName}' is already active.");
                                return;
                            }
                            else
                            {
                                // You have a view related to a different loading unit respect the current one displayed.
                                // The active view is disappeared and it is forced the re-appearance of this view (the view displays the
                                // correct loading unit data)
                                this.logger.Debug($"Appear| Id current view: {id} - loading unit Id: {loadingUnitId}");
                                activeViewForceToAppear = true;
                            }
                        }
                        else
                        {
                            // If the incoming view to be displayed (ref. data) and the actual active view are not of same type,
                            // the active view is not handled
                            this.logger.Warn($"Appear| Requested view '{moduleName}.{viewModelName}' is already active.");
                            return;
                        }
                    }
                    else
                    {
                        // If navigation stack is empty, it forces the displayment of loading unit view
                        this.logger.Warn($"Appear| Requested view '{moduleName}.{viewModelName}' is already active. (navigationStack.Count == 0!)");
                        activeViewForceToAppear = true;
                    }
                }
                else
                {
                    // The active view requested (it is not of Operator.ItemOperations.LOADING_UNIT type) is already active.
                    this.logger.Warn($"Requested view '{moduleName}.{viewModelName}' is already active.");
                    return;
                }
            }

            this.logger.Trace($"Appear| Navigating to view '{moduleName}.{viewModelName}'.");

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
                    this.logger.Trace($"Appear| Marking view '{currentViewRecord.ModuleName}.{currentViewRecord.ViewModelName}' as trackable.");
                    currentViewRecord.IsTrackable = trackCurrentView;

                    this.navigationStack.Where(s => s.ModuleName == moduleName
                                             &&
                                             s.ViewName == viewName
                                             &&
                                             s.IsTrackable).All(ns => ns.IsTrackable = false);
                }

                this.navigationStack.Push(new NavigationHistoryRecord(moduleName, viewName, viewModelName, (int?)data));

                this.eventAggregator
                    .GetEvent<PubSubEvent<NavigationCompletedEventArgs>>()
                    .Publish(new NavigationCompletedEventArgs(moduleName, viewModelName));
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Appear| Cannot show view '{moduleName}.{viewModelName}'. Reason:{ex.Message}");
            }

            // This snippet code is added to force to re-appear the current active view.
            // In this manner the view (of type Operator.ItemOperations.LOADING_UNIT) is updated with the correct loading unit
            // existing in bay (used for internal double bay)
            if (activeViewForceToAppear)
            {
                this.logger.Trace($"Appear| It is forced to re-appear the {viewModelName} view...");
                activeViewModel.OnAppearedAsync();
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
                this.navigationStack.Clear();
                this.Appear(
                        nameof(Ferretto.VW.Utils.Modules.Menu),
                        Ferretto.VW.Utils.Modules.Menu.MAIN_MENU,
                        data: null,
                        trackCurrentView: true);
                return;
            }

            var currentHistoryRecord = this.navigationStack.Pop();
            this.logger.Debug($"Navigating back from '{currentHistoryRecord.ModuleName}.{currentHistoryRecord.ViewName}' ...");

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
            this.logger.Debug($"Navigating go back from '{currentHistoryRecord.ModuleName}.{currentHistoryRecord.ViewName}' ...");

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

        public byte[] TakeScreenshot(bool checkWithPrevious)
        {
            try
            {
                var frameworkElement = Application.Current.MainWindow;
                var relativeBounds = VisualTreeHelper.GetDescendantBounds(frameworkElement);
                var areaWidth = frameworkElement.RenderSize.Width;
                var areaHeight = frameworkElement.RenderSize.Height;
                var xLeft = relativeBounds.X;
                var xRight = xLeft + areaWidth;
                var yTop = relativeBounds.Y;
                var yBottom = yTop + areaHeight;
                var bitmap = new RenderTargetBitmap(
                    (int)Math.Round(xRight, MidpointRounding.AwayFromZero),
                    (int)Math.Round(yBottom, MidpointRounding.AwayFromZero),
                    96, 96, PixelFormats.Default);

                var dv = new DrawingVisual();
                using (var ctx = dv.RenderOpen())
                {
                    var vb = new VisualBrush(frameworkElement);
                    ctx.DrawRectangle(vb, null, new Rect(new Point(xLeft, yTop), new Point(xRight, yBottom)));
                }

                bitmap.Render(dv);

                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 70;
                using (var myStream = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(myStream);
                    var screenshot = myStream.ToArray();
                    var areEqual = screenshot.Length == this.previousScreenshot?.Length;

                    if (checkWithPrevious)
                    {
                        for (var i = 0; i < screenshot.Length && areEqual; i++)
                        {
                            if (screenshot[i] != this.previousScreenshot[i])
                            {
                                areEqual = false;
                            }
                        }

                        if (areEqual)
                        {
                            return Array.Empty<byte>();
                        }
                    }

                    this.previousScreenshot = screenshot;
                    return screenshot;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Cannot get screenshot from MainWindow.");
            }

            return null;
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
            var activeViewModel = this.GetActiveViewModel();
            if (activeViewModel != null)
            {
                this.logger.Debug($"DisappearActiveView| activeView:{activeViewModel.GetType().Name}");
            }
            else
            {
                this.logger.Debug($"DisappearActiveView| activeView: NULL");
            }

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
