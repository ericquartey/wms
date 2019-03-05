using System.Collections.Generic;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class HorizontalPositioningIdleState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineHorizontalPositioning parent;

        #endregion

        #region Constructors

        public HorizontalPositioningIdleState(StateMachineHorizontalPositioning parent, INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;

            this.Profile = new List<ProfilePosition>();

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
        }

        #endregion

        #region Properties

        public int Direction { get; private set; }

        public List<ProfilePosition> Profile { get; private set; }

        public int Target { get; private set; }

        public string Type => "Horizontal Positioning Idle State";

        public int Velocity { get; private set; }

        public float Weight { get; private set; }

        #endregion

        #region Methods

        public void MakeOperation()
        {
            this.driver.ExecuteHorizontalPosition(this.Target, this.Velocity, this.Direction, this.Profile, this.Weight);
        }

        public void ProcessCommandMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void SendCommandMessage(CommandMessage message)
        {
            throw new System.NotImplementedException();
        }

        public void SendNotificationMessage(NotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        private void notifyEventHandler(NotificationMessage notification)
        {
            switch (notification.Status)
            {
                case MessageStatus.OperationEnd:
                    {
                        this.parent.ChangeState(new HorizontalPositioningDoneState(this.parent, this.driver, this.eventAggregator));
                        break;
                    }
                case MessageStatus.OperationError:
                    {
                        this.parent.ChangeState(new HorizontalPositioningErrorState(this.parent, this.driver, this.eventAggregator));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion
    }
}
