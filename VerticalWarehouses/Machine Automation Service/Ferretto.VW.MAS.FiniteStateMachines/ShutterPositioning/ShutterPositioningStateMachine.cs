using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
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

        private readonly Timer delayTimer;

        private readonly InverterIndex inverterIndex;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(
            IEventAggregator eventAggregator,
            IShutterPositioningMessageData shutterPositioningMessageData,
            InverterIndex inverterIndex,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory,
            IMachineSensorsStatus machineSensorsStatus,
            Timer delayTimer)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(logger);

            this.inverterIndex = inverterIndex;

            this.shutterPositioningMessageData = shutterPositioningMessageData;

            this.machineSensorsStatus = machineSensorsStatus;

            this.delayTimer = delayTimer;
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
        public override void Start()
        {
            lock (this.CurrentState)
            {
                if (!this.machineSensorsStatus.IsMachineInNormalState ||
                    this.machineSensorsStatus.IsDrawerPartiallyOnCradleBay1 ||
                    !(this.shutterPositioningMessageData.MovementMode == MovementMode.Position || this.shutterPositioningMessageData.MovementMode == MovementMode.TestLoop)
                    )
                {
                    this.CurrentState = new ShutterPositioningErrorState(this, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, null, this.Logger);
                }
                else
                {
                    this.CurrentState = new ShutterPositioningStartState(this, this.shutterPositioningMessageData, this.inverterIndex, this.Logger, this.machineSensorsStatus, this.delayTimer);
                }

                this.CurrentState?.Start();
            }

            this.Logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
