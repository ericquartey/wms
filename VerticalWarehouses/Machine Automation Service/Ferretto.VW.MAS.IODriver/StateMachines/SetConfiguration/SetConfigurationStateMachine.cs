using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SetConfiguration
{
    internal sealed class SetConfigurationStateMachine : IoStateMachineBase
    {
        #region Fields

        private readonly IoIndex index;

        private readonly IoStatus status;

        #endregion

        #region Constructors

        public SetConfigurationStateMachine(
            BlockingConcurrentQueue<IoWriteMessage> ioCommandQueue,
            IoStatus status,
            IoIndex index,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, ioCommandQueue, serviceScopeFactory)
        {
            this.status = status;
            this.index = index;

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            base.ProcessResponseMessage(message);
        }

        public override void Start()
        {
            this.ChangeState(new SetConfigurationStartState(this, this.status, this.index, this.Logger));
        }

        #endregion
    }
}
