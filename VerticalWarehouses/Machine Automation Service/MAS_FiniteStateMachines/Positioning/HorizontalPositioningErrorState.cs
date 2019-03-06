using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_InverterDriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Positioning
{
    public class HorizontalPositioningErrorState : IState
    {
        #region Fields

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private StateMachineHorizontalPositioning parent;

        #endregion

        #region Constructors

        public HorizontalPositioningErrorState(StateMachineHorizontalPositioning parent, INewInverterDriver driver, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string Type => "Horizontal Positioning Error State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
            throw new System.NotImplementedException();
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

        #endregion
    }
}
