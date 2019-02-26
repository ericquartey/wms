using System;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.Homing;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public class StateMachineHoming : IState, IStateMachine
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly INewRemoteIODriver remoteIODriver;

        private IState state;

        #endregion

        #region Constructors

        public StateMachineHoming(INewInverterDriver driver, INewRemoteIODriver remoteIODriver,
            IEventAggregator eventAggregator)
        {
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.data = null;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public String Type => this.state.Type;

        public Boolean HomingComplete { get; set; }

        public Boolean HorizontalHomingAlreadyDone { get; set; }

        #endregion

        #region Methods

        public void ChangeState(IState newState, CommandMessage message = null)
        {
            this.state = newState;
        }

        public void MakeOperation()
        {
            this.state?.MakeOperation();
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void PublishCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void PublishNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            this.HorizontalHomingAlreadyDone = false;
            this.HomingComplete = false;

            this.state = new HomingIdleState(this, this.driver, this.remoteIODriver, this.data, this.eventAggregator);
            this.state.MakeOperation();
        }

        public void Stop()
        {
            this.state?.Stop();
        }

        #endregion
    }
}
