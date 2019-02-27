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
    public class HorizontalHomingDoneState : IState
    {
        #region Fields

        private readonly IWriteLogService data;

        private readonly INewInverterDriver driver;

        private readonly IEventAggregator eventAggregator;

        private readonly StateMachineHoming parent;

        private readonly INewRemoteIODriver remoteIODriver;

        #endregion

        #region Constructors

        public HorizontalHomingDoneState(StateMachineHoming parent, INewInverterDriver iDriver,
            INewRemoteIODriver remoteIODriver, IWriteLogService iWriteLogService, IEventAggregator eventAggregator)
        {
            this.parent = parent;
            this.driver = iDriver;
            this.remoteIODriver = remoteIODriver;
            this.data = iWriteLogService;
            this.eventAggregator = eventAggregator;

            if (!this.parent.HorizontalHomingAlreadyDone)
                this.eventAggregator.GetEvent<NotificationEvent>().Subscribe(this.notifyEventHandler);
            else
                this.parent.ChangeState(new HomingDoneState(this.parent, this.driver, this.remoteIODriver, this.data,
                    this.eventAggregator));
        }

        #endregion

        #region Properties

        public string Type => "Horizontal homing done";

        #endregion

        #region Methods

        public void MakeOperation()
        {
            if (!this.parent.HorizontalHomingAlreadyDone)
            {
                this.parent.HorizontalHomingAlreadyDone = true;
                this.remoteIODriver.SwitchHorizontalToVertical();
            }
        }

        public void NotifyMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Unsubscribe(this.notifyEventHandler);

            var notifyEvent = new NotificationMessage(null, "Homing Stopped", MessageActor.Any,
                MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationStop);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notifyEvent);
        }

        private void notifyEventHandler(NotificationMessage notification)
        {
            if (notification.Type == MessageType.SwitchHorizontalToVertical)
                switch (notification.Status)
                {
                    case MessageStatus.OperationEnd:
                    {
                        this.parent.ChangeState(new VerticalSwitchDoneState(this.parent, this.driver,
                            this.remoteIODriver, this.data, this.eventAggregator));
                        this.parent.MakeOperation();

                        break;
                    }
                    case MessageStatus.OperationError:
                    {
                        this.parent.ChangeState(new HomingErrorState(this.parent, this.driver, this.remoteIODriver,
                            this.data, this.eventAggregator));
                        break;
                    }
                }

            this.eventAggregator.GetEvent<NotificationEvent>().Unsubscribe(this.notifyEventHandler);
        }

        #endregion
    }
}
