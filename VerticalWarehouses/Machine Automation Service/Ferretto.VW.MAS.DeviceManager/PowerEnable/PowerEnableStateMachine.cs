using System.Text;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Models;
using Ferretto.VW.MAS.DeviceManager.Providers;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.PowerEnable
{
    internal class PowerEnableStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IPowerEnableMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerEnableStateMachine(
            CommandMessage receivedMessage,
            IMachineResourcesProvider machineResourcesProvider,
            IBaysProvider baysProvider,
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory
            )
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.CurrentState = new EmptyState(this.Logger);

            this.baysProvider = baysProvider;

            if (receivedMessage.Data is IPowerEnableMessageData data)
            {
                this.machineData = new PowerEnableMachineData(data.Enable,
                    receivedMessage.RequestingBay,
                    receivedMessage.TargetBay,
                    machineResourcesProvider,
                    eventAggregator,
                    logger,
                    serviceScopeFactory);
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            lock (this.CurrentState)
            {
                var stateData = new PowerEnableStateData(this, this.machineData);
                if (this.machineData.Enable)
                {
                    if (!this.IsMarchPossible(out string errorText))
                    {
                        //TODO This will be double notification either remove this publish or do no enter in error state
                        //var notificationMessage = new NotificationMessage(
                        //    null,
                        //    errorText,
                        //    MessageActor.Any,
                        //    MessageActor.FiniteStateMachines,
                        //    MessageType.InverterException,
                        //    this.machineData.RequestingBay,
                        //    this.machineData.TargetBay,
                        //    MessageStatus.OperationStart);

                        //this.PublishNotificationMessage(notificationMessage);

                        this.Logger.LogError(errorText);

                        this.CurrentState = new PowerEnableErrorState(stateData);
                    }
                    else
                    {
                        this.CurrentState = new PowerEnableStartState(stateData);
                    }
                }
                else
                {
                    this.CurrentState = new PowerEnableStartState(stateData);
                }
                this.CurrentState?.Start();
            }

            this.Logger.LogTrace($"1:CurrentState{this.CurrentState.GetType()}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        private bool IsMarchPossible(out string errorText)
        {
            bool isMarchPossible = true;
            var reason = new StringBuilder();
            foreach (var bay in this.baysProvider.GetAll())
            {
                switch (bay.Number)
                {
                    case BayNumber.BayOne:
                        if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MushroomEmergencyButtonBay1])
                        {
                            isMarchPossible = false;
                            reason.Append("Emergency Active Bay1; ");
                        }
                        if (!this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterLeftSideBay1])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay1 Left; ");
                        }
                        if (!this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterRightSideBay1])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay1 Right; ");
                        }
                        break;

                    case BayNumber.BayTwo:
                        if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MushroomEmergencyButtonBay2])
                        {
                            isMarchPossible = false;
                            reason.Append("Emergency Active Bay2; ");
                        }
                        if (!this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterLeftSideBay2])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay2 Left; ");
                        }
                        if (!this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterRightSideBay2])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay2 Right; ");
                        }
                        break;

                    case BayNumber.BayThree:
                        if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MushroomEmergencyButtonBay3])
                        {
                            isMarchPossible = false;
                            reason.Append("Emergency Active Bay3; ");
                        }
                        if (!this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterLeftSideBay3])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay3 Left; ");
                        }
                        if (!this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterRightSideBay3])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay3 Right; ");
                        }
                        break;

                    default:
                        break;
                }
            }

            errorText = reason.ToString();
            return isMarchPossible;
        }

        #endregion
    }
}
