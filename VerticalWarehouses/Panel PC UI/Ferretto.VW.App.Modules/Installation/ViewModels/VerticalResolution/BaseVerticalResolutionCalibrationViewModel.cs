using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class BaseVerticalResolutionCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly IMachineResolutionCalibrationProcedureService resolutionCalibrationService;

        private decimal? currentResolution;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BaseVerticalResolutionCalibrationViewModel(
            IEventAggregator eventAggregator,
            IMachineResolutionCalibrationProcedureService resolutionCalibrationService)
            : base(Services.PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (resolutionCalibrationService is null)
            {
                throw new ArgumentNullException(nameof(resolutionCalibrationService));
            }

            this.InitializeNavigationMenu();
            this.eventAggregator = eventAggregator;
            this.resolutionCalibrationService = resolutionCalibrationService;
        }

        #endregion

        #region Properties

        public decimal? CurrentResolution
        {
            get => this.currentResolution;
            protected set => this.SetProperty(ref this.currentResolution, value);
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            protected set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public IMachineResolutionCalibrationProcedureService ResolutionCalibrationService => this.resolutionCalibrationService;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.subscriptionToken != null)
            {
                this.eventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.eventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    message => this.OnAutomationMessageReceived(message),
                    ThreadOption.UIThread,
                    false);
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            this.ShowPrevStep(false, false);
            this.ShowNextStep(false, false);
            this.ShowAbortStep(false, false);
        }

        protected abstract void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message);

        protected abstract void RaiseCanExecuteChanged();

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.VerticalResolutionCalibration.STEP1,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step1,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.VerticalResolutionCalibration.STEP2,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step2,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.VerticalResolutionCalibration.STEP3,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Step3,
                    trackCurrentView: false));
        }

        #endregion
    }
}
