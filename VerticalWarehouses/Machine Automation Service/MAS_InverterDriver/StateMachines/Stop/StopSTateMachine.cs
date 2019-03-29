using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.InverterDriver.StateMachines.Stop
{
    public class StopStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public StopStateMachine(Axis axisToStop, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            this.axisToStop = axisToStop;
            this.inverterCommandQueue = inverterCommandQueue;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
        }

        #endregion

        #region Destructors

        ~StopStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void OnPublishNotification(NotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new StopState(this, this.axisToStop, this.logger);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
