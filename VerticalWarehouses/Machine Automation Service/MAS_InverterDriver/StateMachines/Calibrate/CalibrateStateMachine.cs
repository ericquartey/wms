using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Utilities;
using Ferretto.VW.MAS_InverterDriver.StateMachines.Calibrate;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public class CalibrateStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private Axis currentAxis;

        private bool disposed;

        #endregion

        #region Constructors

        public CalibrateStateMachine(Axis axisToCalibrate, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.inverterCommandQueue = inverterCommandQueue;
        }

        #endregion

        #region Destructors

        ~CalibrateStateMachine()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        public override void ChangeState(IInverterState newState)
        {
            if (newState is EndState)
                if (this.axisToCalibrate == Axis.Both && this.currentAxis == Axis.Horizontal)
                {
                    this.currentAxis = Axis.Vertical;

                    SwitchAxisMessageData switchAxisiMessageData = new SwitchAxisMessageData(this.currentAxis);
                    CommandMessage switchAxisCommandMessage = new CommandMessage(switchAxisiMessageData, "Switch Axis to calibrate", MessageActor.IODriver, MessageActor.InverterDriver, MessageType.SwitchAxis);
                    this.eventAggregator.GetEvent<CommandEvent>().Publish(switchAxisCommandMessage);
                    return;
                }

            base.ChangeState(newState);
        }

        public override void Start()
        {
            switch (this.axisToCalibrate)
            {
                case Axis.Both:
                case Axis.Horizontal:
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Vertical:
                    this.currentAxis = Axis.Vertical;
                    break;
            }

            this.CurrentState = new VoltageDisabledState(this, this.currentAxis);
        }

        #endregion
    }
}
