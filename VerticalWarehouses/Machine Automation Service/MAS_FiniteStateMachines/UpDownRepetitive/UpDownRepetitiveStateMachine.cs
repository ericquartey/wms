using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_FiniteStateMachines.UpDownRepetitive
{
    public class UpDownRepetitiveStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IUpDownRepetitiveMessageData upDownMessageData;

        private readonly ILogger logger;

        private bool IsStopRequested;

        private int NumberOfCompletedHalfCycles;

        private int NumberOfRequestedCycles;

        #endregion

        #region Constructors

        public UpDownRepetitiveStateMachine(IEventAggregator eventAggregator, IUpDownRepetitiveMessageData upDownMessageData, ILogger logger)
                                    : base(eventAggregator, null)
        {
            logger.LogDebug("1:Method Start");

            this.logger = logger;
            this.upDownMessageData = upDownMessageData;
            this.NumberOfRequestedCycles = upDownMessageData.NumberOfRequiredCycles;
            this.IsStopRequested = false;
            //this.OperationDone = false;

            
        }

        #endregion

        #region Properties

        public IState GetState => this.CurrentState;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Stop:
                    //TODO Add business logic to stop current action at state machine level
                    this.IsStopRequested = true;
                    break;

                default:
                    break;
            }

            this.CurrentState?.ProcessCommandMessage(message);
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            //if (message.Type == MessageType.Positioning)
            //{
            //    switch (message.Status)
            //    {
            //        case MessageStatus.OperationEnd:
            //            //TEMP Add business logic after the positioning operation is done successfully
            //            this.NumberOfCompletedHalfCycles++;

            //            var numberOfCompletedCycles = this.NumberOfCompletedHalfCycles / 2;
            //            this.OperationDone = (this.NumberOfRequestedCycles == numberOfCompletedCycles);

            //            break;

            //        case MessageStatus.OperationError:
            //            //TEMP Add business logic after an error occurs
            //            break;

            //        default:
            //            break;
            //    }
            //}

            this.CurrentState?.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void PublishNotificationMessage(NotificationMessage message)
        {
            switch (message.Type)
            {
                //case MessageType.UpDown:
                //    {
                //        var numberOfCompletedCycles = this.NumberOfCompletedHalfCycles / 2;
                //        var upDownMessage = new UpDownRepetitiveNotificationMessageData(numberOfCompletedCycles);

                //        //TEMP Send a notification about the up&down progression to all the world
                //        var newMessage = new NotificationMessage(upDownMessage,
                //            "Up&Down executing",
                //            MessageActor.Any,
                //            MessageActor.FiniteStateMachines,
                //            MessageType.UpDown,
                //            MessageStatus.OperationExecuting,
                //            ErrorLevel.NoError,
                //            MessageVerbosity.Info);

                //        this.EventAggregator.GetEvent<NotificationEvent>().Publish(newMessage);
                //        break;
                //    }

                case MessageType.Stop:
                    {
                        var msgStatus = (this.IsStopRequested) ? MessageStatus.OperationStop : MessageStatus.OperationEnd;
                        //TEMP Send a notification about the end (/stop) operation to all the world
                        var newMessage = new NotificationMessage(null,
                            "Up&Down End",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.Stop,
                            msgStatus,
                            ErrorLevel.NoError,
                            MessageVerbosity.Info);

                        this.EventAggregator.GetEvent<NotificationEvent>().Publish(newMessage);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <inheritdoc/>
        public override void Start()
        {
            this.CurrentState = new UpDownStartState(this, this.upDownMessageData, this.logger);
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
