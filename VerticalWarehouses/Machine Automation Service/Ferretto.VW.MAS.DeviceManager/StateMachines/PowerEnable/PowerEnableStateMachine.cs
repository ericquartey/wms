using System.Text;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Interfaces;
using Ferretto.VW.MAS.DeviceManager.PowerEnable.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.PowerEnable
{
    internal class PowerEnableStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IPowerEnableMachineData machineData;

        #endregion

        #region Constructors

        public PowerEnableStateMachine(
            CommandMessage receivedMessage,
            IMachineResourcesProvider machineResourcesProvider,
            IBaysDataProvider baysDataProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.baysDataProvider = baysDataProvider;

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
                    if (!this.IsMarchPossible(out var errorText))
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

                        this.ChangeState(new PowerEnableErrorState(stateData, this.Logger));
                    }
                    else
                    {
                        this.ChangeState(new PowerEnableStartState(stateData, this.Logger));
                    }
                }
                else
                {
                    this.ChangeState(new PowerEnableStartState(stateData, this.Logger));
                }
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        private bool IsMarchPossible(out string errorText)
        {
            var isMarchPossible = true;
            var reason = new StringBuilder();

            if (this.machineData.MachineSensorStatus.FireAlarm)
            {
                isMarchPossible = false;
                reason.Append("Fire alarm active; ");
            }

            foreach (var bay in this.baysDataProvider.GetAll())
            {
                switch (bay.Number)
                {
                    case BayNumber.BayOne:
                        if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MushroomEmergencyButtonBay1])
                        {
                            isMarchPossible = false;
                            reason.Append("Emergency Active Bay1; ");
                        }
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.AntiIntrusionBarrierBay1])
                        {
                            //isMarchPossible = false;
                            reason.Append("Anti Intrusion Barrier Active Bay1; ");
                        }
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterLeftSideBay1])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay1 Left; ");
                        }
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterRightSideBay1])
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
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.AntiIntrusionBarrierBay2])
                        {
                            //isMarchPossible = false;
                            reason.Append("Anti Intrusion Barrier Active Bay2; ");
                        }
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterLeftSideBay2])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay2 Left; ");
                        }
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterRightSideBay2])
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
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.AntiIntrusionBarrierBay3])
                        {
                            //isMarchPossible = false;
                            reason.Append("Anti Intrusion Barrier Active Bay3; ");
                        }
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterLeftSideBay3])
                        {
                            isMarchPossible = false;
                            reason.Append("Micro Carter Active Bay3 Left; ");
                        }
                        else if (this.machineData.MachineSensorStatus.DisplayedInputs[(int)IOMachineSensors.MicroCarterRightSideBay3])
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
