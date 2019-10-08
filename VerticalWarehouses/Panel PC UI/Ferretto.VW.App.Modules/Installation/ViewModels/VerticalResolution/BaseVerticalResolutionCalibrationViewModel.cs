using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
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

        private double? currentPosition;

        private decimal? currentResolution;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private DelegateCommand stopCommand;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public BaseVerticalResolutionCalibrationViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorService machineElevatorService,
            IMachineResolutionCalibrationProcedureService resolutionCalibrationService)
            : base(Services.PresentationMode.Installer)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (machineElevatorService is null)
            {
                throw new ArgumentNullException(nameof(machineElevatorService));
            }

            if (resolutionCalibrationService is null)
            {
                throw new ArgumentNullException(nameof(resolutionCalibrationService));
            }

            this.eventAggregator = eventAggregator;
            this.MachineElevatorService = machineElevatorService;
            this.resolutionCalibrationService = resolutionCalibrationService;

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public double? CurrentPosition
        {
            get => this.currentPosition;
            protected set => this.SetProperty(ref this.currentPosition, value);
        }

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
                    if (this.isExecutingProcedure)
                    {
                        this.ClearNotifications();
                    }

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

        public IMachineElevatorService MachineElevatorService { get; }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public IMachineResolutionCalibrationProcedureService ResolutionCalibrationService => this.resolutionCalibrationService;

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

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

            this.subscriptionToken = this.eventAggregator
                .GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                    message => this.OnAutomationMessageReceived(message),
                    ThreadOption.UIThread,
                    false);

            await this.RetrieveCurrentPositionAsync();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            this.ShowPrevStep(false, false);
            this.ShowNextStep(false, false);
            this.ShowAbortStep(false, false);
        }

        protected virtual void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message is null || message.Data is null)
            {
                return;
            }

            if (message.Data.AxisMovement == Axis.Vertical)
            {
                this.CurrentPosition = message?.Data?.CurrentPosition ?? this.CurrentPosition;

                this.IsExecutingProcedure =
                    message.Status != MessageStatus.OperationEnd
                    &&
                    message.Status != MessageStatus.OperationStop;

                if (message.Status == MessageStatus.OperationStop)
                {
                    this.ShowNotification(
                        VW.App.Resources.InstallationApp.ProcedureWasStopped,
                        Services.Models.NotificationSeverity.Warning);
                }
            }
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            this.stopCommand?.RaiseCanExecuteChanged();
        }

        private bool CanStop()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

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

        private async Task RetrieveCurrentPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.CurrentPosition = await this.MachineElevatorService.GetVerticalPositionAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.MachineElevatorService.StopAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
