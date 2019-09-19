using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Template
{
    internal class TemplateStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ITemplateData machineData;

        #endregion

        #region Constructors

        public TemplateStateMachine(
            ITemplateData machineData)
            : base(machineData.EventAggregator, machineData.Logger, machineData.ServiceScopeFactory)
        {
            this.CurrentState = new EmptyState(machineData.Logger);

            this.machineData = machineData;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.CurrentState.ProcessCommandMessage(message);
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public override void Start()
        {
            lock (this.CurrentState)
            {
                this.CurrentState = new TemplateStartState(this, this.machineData);
                this.CurrentState?.Start();
            }
        }

        public override void Stop()
        {
            lock (this.CurrentState)
            {
                this.CurrentState.Stop();
            }
        }

        #endregion
    }
}
