using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableSetParametersState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private int parameterId;

        private InverterTableIndex tableIndex;

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

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace("1:Method Start");

            this.tableIndex = InverterTableIndex.TableTravelP7;
            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
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

                switch (message.ParameterId)
                {
                    case InverterParameterId.TableTravelTableIndex:
                        switch (this.tableIndex)
                        {
                            case InverterTableIndex.TableTravelP7:
                                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetPosition, this.data.TargetPosition));
                                this.Logger.LogDebug($"Set target position: {this.data.TargetPosition}");
                                break;

                            case InverterTableIndex.TableTravelSet1:
                            case InverterTableIndex.TableTravelSet2:
                            case InverterTableIndex.TableTravelSet3:
                            case InverterTableIndex.TableTravelSet4:
                            case InverterTableIndex.TableTravelSet5:
                                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetSpeeds, this.data.TargetSpeed[this.parameterId]));
                                this.Logger.LogDebug($"Set target Speed[{this.parameterId}]: {this.data.TargetSpeed[this.parameterId]}: table index {this.tableIndex}");
                                break;

                            case InverterTableIndex.TableTravelP1:
                            case InverterTableIndex.TableTravelP2:
                            case InverterTableIndex.TableTravelP3:
                            case InverterTableIndex.TableTravelP4:
                                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetPosition, this.data.SwitchPosition[this.parameterId]));
                                this.Logger.LogDebug($"Set Switch Position[{this.parameterId}]: {this.data.SwitchPosition[this.parameterId]}: table index {this.tableIndex}");
                                break;
                        }
                        break;

                    case InverterParameterId.TableTravelTargetPosition:
                        if (this.tableIndex == InverterTableIndex.TableTravelDirection)
                        {
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelDirection, (short)this.data.Direction));
                            this.Logger.LogDebug($"Set Direction: {this.data.Direction}");
                        }
                        else if (++this.parameterId < this.data.SwitchPosition.Length - 1)
                        {
                            this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelP1 + this.parameterId);
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(
                                new PositioningTableEnableOperationState(
                                    this.ParentStateMachine,
                                    this.data,
                                    this.InverterStatus as IPositioningInverterStatus,
                                    this.Logger));
                        }
                        break;

                    case InverterParameterId.TableTravelDirection:
                        if (!this.data.RefreshAll)
                        {
                            this.ParentStateMachine.ChangeState(
                                new PositioningTableEnableOperationState(
                                    this.ParentStateMachine,
                                    this.data,
                                    this.InverterStatus as IPositioningInverterStatus,
                                    this.Logger));
                        }
                        else
                        {
                            this.parameterId = 0;
                            this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
                        }
                        break;

                    case InverterParameterId.TableTravelTargetSpeeds:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetAccelerations, this.data.TargetAcceleration[this.parameterId]));
                        this.Logger.LogDebug($"Set Acceleration[{this.parameterId}]: {this.data.TargetAcceleration[this.parameterId]}: table index {this.tableIndex}");

                        break;

                    case InverterParameterId.TableTravelTargetAccelerations:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetDecelerations, this.data.TargetAcceleration[this.parameterId]));
                        this.Logger.LogDebug($"Set Deceleration[{this.parameterId}]: {this.data.TargetAcceleration[this.parameterId]}: table index {this.tableIndex}");

                        break;

                    case InverterParameterId.TableTravelTargetDecelerations:
                        if (++this.parameterId < this.data.SwitchPosition.Length)
                        {
                            this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
                        }
                        else
                        {
                            this.parameterId = 0;
                            this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelP1 + this.parameterId);
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
                        }
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
