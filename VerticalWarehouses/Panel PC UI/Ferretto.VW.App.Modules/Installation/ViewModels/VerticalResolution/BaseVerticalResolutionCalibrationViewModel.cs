using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class BaseVerticalResolutionCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly IMachineResolutionCalibrationProcedureService resolutionCalibrationService;

        private VerticalResolutionCalibrationData calibrationData;

        private decimal? currentResolution;

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

        public VerticalResolutionCalibrationData CalibrationData
        {
            get => this.calibrationData;
            set => this.SetProperty(ref this.calibrationData, value);
        }

        public decimal? CurrentResolution
        {
            get => this.currentResolution;
            set => this.SetProperty(ref this.currentResolution, value);
        }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public IMachineResolutionCalibrationProcedureService ResolutionCalibrationService => this.resolutionCalibrationService;

        #endregion

        #region Methods

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

        protected abstract void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message);

        protected override void OnDispose()
        {
            base.OnDispose();

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

        #endregion
    }
}
