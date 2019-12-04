using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using NLog;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class MachineService : IMachineService, IDisposable
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachinePowerWebService machinePowerWebService;

        private readonly ISensorsService sensorsService;

        private readonly IMachineShuttersWebService shuttersWebService;

        private bool isDisposed;

        private bool isHoming;

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

            this.receiveHomingUpdateToken = this.eventAggregator
                    .GetEvent<NotificationEventUI<HomingMessageData>>()
                    .Subscribe(
                        async (m) => await this.OnHomingProcedureStatusChanged(m),
                        ThreadOption.UIThread,
                        false);
        }

        #endregion

        #region Properties

        public bool IsHoming { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);

            this.receiveHomingUpdateToken?.Dispose();
            this.receiveHomingUpdateToken = null;
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

        #endregion
    }
}
