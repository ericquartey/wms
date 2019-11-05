using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Models;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ShutterPositioning
{
    internal class ShutterPositioningStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IShutterPositioningMachineData machineData;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(
            IShutterPositioningMessageData positioningMessageData,
            BayNumber requestingBay,
            BayNumber targetBay,
            InverterIndex inverterIndex,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineData = new ShutterPositioningMachineData(
                positioningMessageData,
                requestingBay,
                targetBay,
                inverterIndex,
                machineResourcesProvider,
                eventAggregator,
                logger,
                serviceScopeFactory);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Publish Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            base.PublishNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var setupProceduresProvider = scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
                this.machineData.PositioningMessageData.PerformedCycles = setupProceduresProvider.GetShutterTest().PerformedCycles;
            }

            var stateData = new ShutterPositioningStateData(this, this.machineData);

            if (this.machineData.MachineSensorsStatus.IsDrawerPartiallyOnCradleBay1)
            {
                this.Logger.LogError($"Invalid Elevator presence sensors before moving shutter");
                this.ChangeState(new ShutterPositioningErrorState(stateData));
            }
            if (!(this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterPosition || this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterTest))
            {
                this.Logger.LogError($"Invalid positioning message");
                this.ChangeState(new ShutterPositioningErrorState(stateData));
            }
            else
            {
                this.ChangeState(new ShutterPositioningStartState(stateData));
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        #endregion
    }
}
