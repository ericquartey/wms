using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;
using Axis = Ferretto.VW.CommonUtils.Messages.Enumerations.Axis;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal abstract class BaseVerticalResolutionCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineElevatorService machineElevatorService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService;

        private decimal? currentResolution;

        private bool isExecutingProcedure;

        private SubscriptionToken positioningOperationChangedToken;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public BaseVerticalResolutionCalibrationViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineElevatorService machineElevatorService,
            IHealthProbeService healthProbeService)
            : base(PresentationMode.Installer)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorService = machineElevatorService ?? throw new ArgumentNullException(nameof(machineElevatorService));
            this.MachineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.resolutionCalibrationWebService = resolutionCalibrationWebService ?? throw new ArgumentNullException(nameof(resolutionCalibrationWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
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
                    if (this.isExecutingProcedure)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public override bool IsWaitingForResponse
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

        public IMachineElevatorWebService MachineElevatorWebService { get; }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public IMachineVerticalResolutionCalibrationProcedureWebService ResolutionCalibrationService => this.resolutionCalibrationWebService;

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanStop));

        internal IEventAggregator EventAggregator => this.eventAggregator;

        internal IHealthProbeService HealthProbeService => this.healthProbeService;

        protected VerticalResolutionCalibrationProcedure ProcedureParameters { get; private set; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;
            */
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.positioningOperationChangedToken = this.positioningOperationChangedToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Subscribe(
                        this.OnPositioningOperationChanged,
                        ThreadOption.UIThread,
                        false);

            await this.RetrieveProcedureParametersAsync();
        }

        protected virtual void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.IsErrored())
            {
                this.ShowNotification(
                    VW.App.Resources.InstallationApp.ProcedureWasStopped,
                    Services.Models.NotificationSeverity.Warning);
            }

            if (message.Data?.AxisMovement != Axis.Vertical)
            {
                return;
            }

            if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.stopCommand?.RaiseCanExecuteChanged();
        }

        private bool CanStop()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private async Task RetrieveProcedureParametersAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.ProcedureParameters = await this.resolutionCalibrationWebService.GetParametersAsync();
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
                await this.MachineElevatorWebService.StopAsync();
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
