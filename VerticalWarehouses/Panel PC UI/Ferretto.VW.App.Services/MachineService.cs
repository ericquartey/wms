using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services
{
    internal sealed class MachineService : BindableBase, IMachineService, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly ISensorsService sensorsService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private SubscriptionToken bayChainPositionChangedToken;

        private SubscriptionToken elevatorPositionChangedToken;

        private bool isDisposed;

        private bool isHoming;

        private MachineStatus machineStatus;

        private SubscriptionToken receiveHomingUpdateToken;

        #endregion

        #region Constructors

        public MachineService(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineShuttersWebService shuttersWebService,
            IMachineCarouselWebService machineCarouselWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachinePowerWebService machinePowerWebService,
            ISensorsService sensorsService)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));
            this.shuttersWebService = shuttersWebService ?? throw new ArgumentNullException(nameof(shuttersWebService));
            this.machineCarouselWebService = machineCarouselWebService ?? throw new ArgumentNullException(nameof(machineCarouselWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machinePowerWebService = machinePowerWebService ?? throw new ArgumentNullException(nameof(machinePowerWebService));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));

            this.machineStatus = new MachineStatus();

            this.receiveHomingUpdateToken = this.eventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        async (m) => await this.OnHomingProcedureStatusChanged(m),
                        ThreadOption.UIThread,
                        false);

            this.elevatorPositionChangedToken = this.elevatorPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnElevatorPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.bayChainPositionChangedToken = this.bayChainPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnBayChainPositionChanged,
                        ThreadOption.UIThread,
                        false);
        }

        #endregion

        #region Properties

        public bool IsHoming
        {
            get => this.isHoming;
            set => this.SetProperty(ref this.isHoming, value);
        }

        public MachineStatus MachineStatus
        {
            get => this.machineStatus;
            set => this.SetProperty(ref this.machineStatus, value, this.MachineStatusNotificationProperty);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);

            this.receiveHomingUpdateToken?.Dispose();
            this.receiveHomingUpdateToken = null;

            this.elevatorPositionChangedToken?.Dispose();
            this.elevatorPositionChangedToken = null;

            this.bayChainPositionChangedToken?.Dispose();
            this.bayChainPositionChangedToken = null;
        }

        public async Task StopMovingByAllAsync()
        {
            //this.machineLoadingUnitsWebService?.StopAsync();
            this.machineElevatorWebService?.StopAsync();
            this.machineCarouselWebService?.StopAsync();
            this.shuttersWebService?.StopAsync();
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.isDisposed = true;
        }

        private void MachineStatusNotificationProperty()
        {
            this.eventAggregator
                .GetEvent<MachineStatusChangedPubSubEvent>()
                .Publish(new MachineStatusChangedMessage(this.MachineStatus));
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.UpdateMachineStatus(e);
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.UpdateMachineStatus(e);
        }

        private async Task OnHomingProcedureStatusChanged(NotificationMessageUI<HomingMessageData> message)
        {
            var isHoming = await this.machinePowerWebService.GetIsHomingAsync();

            if (isHoming != this.IsHoming ||
                isHoming && message?.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd ||
                !isHoming && message?.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationError)
            {
                this.eventAggregator
                    .GetEvent<HomingChangedPubSubEvent>()
                    .Publish(new HomingChangedMessage(isHoming));
            }

            this.IsHoming = isHoming;
        }

        private void UpdateMachineStatus(EventArgs e)
        {
            var ms = new MachineStatus();

            if (e is ElevatorPositionChangedEventArgs dataElevatorPosition)
            {
                this.sensorsService.RetrieveElevatorPosition(
                    new ElevatorPosition
                    {
                        Horizontal = dataElevatorPosition.HorizontalPosition,
                        Vertical = dataElevatorPosition.VerticalPosition,
                        BayPositionId = dataElevatorPosition.BayPositionId,
                        CellId = dataElevatorPosition.CellId
                    });
                ms.ElevatorLogicalPosition = this.sensorsService?.ElevatorLogicalPosition;
            }
            else
            {
                ms.ElevatorLogicalPosition = this.MachineStatus.ElevatorLogicalPosition;
            }

            if (e is BayChainPositionChangedEventArgs dataBayChainPosition)
            {
                ms.BayChainPosition = dataBayChainPosition.Position;
            }
            else
            {
                ms.BayChainPosition = this.MachineStatus.BayChainPosition;
            }

            this.MachineStatus = ms;
        }

        #endregion
    }
}
