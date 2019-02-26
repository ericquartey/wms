using System;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly StateMachineHoming parent;

        private readonly INewRemoteIODriver remoteIODriver;

        #endregion

        #region Constructors

        public HomingDoneState(StateMachineHoming parent, INewInverterDriver driver, INewRemoteIODriver remoteIODriver,
            IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = driver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            this.parent.HomingComplete = true;

            this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);

            this.remoteIODriver.SwitchHorizontalToVertical();
        }

        #endregion

        #region Properties

        public String Type => "Homing Done State";

        #endregion

        #region Methods

        public void MakeOperation()
        {
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
        }

        private void notifyEventHandler(NotificationMessage notification)
        {
            if (notification.Type == MessageType.SwitchHorizontalToVertical)
                switch (notification.Status)
                {
                    case MessageStatus.OperationEnd:
                    {
                        var notifyEvent = new NotificationMessage(null, "Homing Done", MessageActor.Any,
                            MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationEnd);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);

                        break;
                    }
                    case MessageStatus.OperationError:
                    {
                        break;
                    }
                }

            this.eventAggregator.GetEvent<NotificationEvent>().Unsubscribe(this.notifyEventHandler);
        }

        #endregion
    }
}
