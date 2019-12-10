using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class MachineElevatorService : IMachineElevatorService, IDisposable
    {
        #region Fields

        private readonly SubscriptionToken elevatorPositionChangedToken;

        private readonly IEventAggregator eventAggregator;

        private readonly SubscriptionToken healthStatusChangedToken;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private bool isDisposed;

        #endregion

        #region Constructors

        public MachineElevatorService(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineElevatorWebService = machineElevatorWebService ?? throw new ArgumentNullException(nameof(machineElevatorWebService));

            this.elevatorPositionChangedToken = this.eventAggregator
              .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
              .Subscribe(
                  this.OnElevatorPositionChanged,
                  ThreadOption.UIThread,
                  false);

            this.healthStatusChangedToken = this.eventAggregator
                .GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>()
                .Subscribe(
                    async (e) => await this.OnHealthStatusChangedAsync(e),
                    ThreadOption.UIThread,
                    false);

            this.GetElevatorPositionAsync();
        }

        #endregion

        #region Properties

        public ElevatorPosition Position { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        public async Task GetElevatorPositionAsync()
        {
            try
            {
                this.Position = await this.machineElevatorWebService.GetPositionAsync();

                var eventArgs = new ElevatorPositionChangedEventArgs(
                    this.Position.Vertical,
                    this.Position.Horizontal,
                    this.Position.CellId,
                    this.Position.BayPositionId,
                    this.Position.BayPositionUpper);

                this.eventAggregator
                    .GetEvent<PubSubEvent<ElevatorPositionChangedEventArgs>>()
                    .Publish(eventArgs);
            }
            catch
            {
                // do nothing
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.elevatorPositionChangedToken.Dispose();
                this.healthStatusChangedToken.Dispose();
            }

            this.isDisposed = true;
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.logger.Trace($"Elevator X={e.HorizontalPosition:0.0}; Y={e.VerticalPosition:0.0}; Cell={e.CellId}; BayPosition={e.BayPositionId}; BayPositionUpper={e.BayPositionUpper};");

            this.Position = new ElevatorPosition
            {
                Vertical = e.VerticalPosition,
                Horizontal = e.HorizontalPosition,
                CellId = e.CellId,
                BayPositionId = e.BayPositionId,
                BayPositionUpper = e.BayPositionUpper,
            };
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            if (e.HealthStatus == HealthStatus.Healthy
                ||
                e.HealthStatus == HealthStatus.Degraded)
            {
                await this.GetElevatorPositionAsync();
            }
        }

        private void ShowError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        #endregion
    }
}
