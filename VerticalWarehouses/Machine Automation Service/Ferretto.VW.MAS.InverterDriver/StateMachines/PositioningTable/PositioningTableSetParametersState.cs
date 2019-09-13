using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableSetParametersState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private int parameterId;

        #endregion

        #region Constructors

        public PositioningTableSetParametersState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;

            logger.LogDebug("1:Method Start");
        }

        #endregion

        #region Destructors

        ~PositioningTableSetParametersState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Release()
        {
        }

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetPosition, this.data.TargetPosition, InverterDataset.TableTravelP7));
            this.Logger.LogDebug($"Set target position: {this.data.TargetPosition}");
            this.parameterId = 0;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new PositioningTableEndState(this.ParentStateMachine, this.InverterStatus, this.Logger, true));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:ID Parameter={message.ParameterId}");

                InverterDataset dataset;

                switch (message.ParameterId)
                {
                    case InverterParameterId.TableTravelTargetPosition:
                        this.parameterId = 0;
                        dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                        if (this.data.RefreshAll)
                        {
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetSpeeds, this.data.TargetSpeed[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set target Speed[{this.parameterId}]: {this.data.TargetSpeed[this.parameterId]}");
                        }
                        else
                        {
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelSwitchPositions, this.data.SwitchPosition[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set Switch Position[{this.parameterId}]: {this.data.SwitchPosition[this.parameterId]}");
                        }
                        break;

                    case InverterParameterId.TableTravelTargetSpeeds:
                        if (++this.parameterId < this.data.TargetSpeed.Length)
                        {
                            dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetSpeeds, this.data.TargetSpeed[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set target Speed[{this.parameterId}]: {this.data.TargetSpeed[this.parameterId]}");
                        }
                        else
                        {
                            this.parameterId = 0;
                            dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetAccelerations, this.data.TargetAcceleration[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set Acceleration[{this.parameterId}]: {this.data.TargetAcceleration[this.parameterId]}");
                        }
                        break;

                    case InverterParameterId.TableTravelTargetAccelerations:
                        if (++this.parameterId < this.data.TargetAcceleration.Length)
                        {
                            dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetAccelerations, this.data.TargetAcceleration[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set Acceleration[{this.parameterId}]: {this.data.TargetAcceleration[this.parameterId]}");
                        }
                        else
                        {
                            this.parameterId = 0;
                            dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetDecelerations, this.data.TargetDeceleration[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set Deceleration[{this.parameterId}]: {this.data.TargetDeceleration[this.parameterId]}");
                        }
                        break;

                    case InverterParameterId.TableTravelTargetDecelerations:
                        if (++this.parameterId < this.data.TargetDeceleration.Length)
                        {
                            dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetDecelerations, this.data.TargetDeceleration[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set Deceleration[{this.parameterId}]: {this.data.TargetDeceleration[this.parameterId]}");
                        }
                        else
                        {
                            this.parameterId = 0;
                            dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelSwitchPositions, this.data.SwitchPosition[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set Switch Position[{this.parameterId}]: {this.data.SwitchPosition[this.parameterId]}");
                        }
                        break;

                    case InverterParameterId.TableTravelSwitchPositions:
                        if (++this.parameterId < this.data.SwitchPosition.Length)
                        {
                            dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelSwitchPositions, this.data.SwitchPosition[this.parameterId], dataset));
                            this.Logger.LogDebug($"Set Switch Position[{this.parameterId}]: {this.data.SwitchPosition[this.parameterId]}");
                        }
                        else
                        {
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelDirection, this.data.Direction, InverterDataset.TableTravelP7));
                            this.Logger.LogDebug($"Set Direction: {this.data.Direction}");
                        }
                        break;

                    case InverterParameterId.TableTravelDirection:
                        this.ParentStateMachine.ChangeState(new PositioningTableEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus, this.Logger));
                        break;
                }
            }
            return returnValue;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        #endregion
    }
}
