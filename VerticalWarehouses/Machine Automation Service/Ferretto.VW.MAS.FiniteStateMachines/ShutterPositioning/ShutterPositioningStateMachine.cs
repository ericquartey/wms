using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning.Models;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    internal class ShutterPositioningStateMachine : StateMachineBase
    {

        #region Fields

        private readonly IShutterPositioningMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(
            IShutterPositioningMessageData positioningMessageData,
            BayNumber requestingBay,
            BayNumber targetBay,
            InverterIndex inverterIndex,
            IMachineSensorsStatus machineSensorsStatus,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)

            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            this.machineData = new ShutterPositioningMachineData(positioningMessageData, requestingBay, targetBay, inverterIndex, machineSensorsStatus, eventAggregator, logger, serviceScopeFactory);
        }

        #endregion

        #region Destructors

        ~ShutterPositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion



        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock(this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock(this.CurrentState)
            {
                this.CurrentState.ProcessFieldNotificationMessage(message);
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            lock(this.CurrentState)
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
            lock(this.CurrentState)
            {
                var stateData = new ShutterPositioningStateData(this, this.machineData);
                if(!this.machineData.MachineSensorsStatus.IsMachineInRunningState ||
                    this.machineData.MachineSensorsStatus.IsDrawerPartiallyOnCradleBay1 ||
                    !(this.machineData.PositioningMessageData.MovementMode == MovementMode.Position || this.machineData.PositioningMessageData.MovementMode == MovementMode.ShutterTest)
                    )
                {
                    this.CurrentState = new ShutterPositioningErrorState(stateData);
                }
                else
                {
                    this.CurrentState = new ShutterPositioningStartState(stateData);
                }

                this.CurrentState?.Start();
            }

            this.Logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock(this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(this.disposed)
            {
                return;
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
