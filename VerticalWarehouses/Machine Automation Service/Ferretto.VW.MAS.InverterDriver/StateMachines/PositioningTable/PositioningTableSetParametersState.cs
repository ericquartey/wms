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

        private InverterDataset tableIndex;

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

            this.tableIndex = InverterDataset.TableTravelP7;
            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
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

                InverterDataset dataset;

                switch (message.ParameterId)
                {
                    case InverterParameterId.TableTravelTargetPosition:
                        this.parameterId = 0;
                        dataset = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                        if (this.data.RefreshAll)
                        {
                            case InverterDataset.TableTravelP7:
                                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetPosition, this.data.TargetPosition));
                                this.Logger.LogDebug($"Set target position: {this.data.TargetPosition}");
                                break;

                            case InverterDataset.TableTravelSet1:
                            case InverterDataset.TableTravelSet2:
                            case InverterDataset.TableTravelSet3:
                            case InverterDataset.TableTravelSet4:
                            case InverterDataset.TableTravelSet5:
                                this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetSpeeds, this.data.TargetSpeed[this.parameterId]));
                                this.Logger.LogDebug($"Set target Speed[{this.parameterId}]: {this.data.TargetSpeed[this.parameterId]}: dataset {this.tableIndex}");
                                break;
                        }
                        break;

                    case InverterParameterId.TableTravelTargetPosition:
                        this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelDirection, (short)this.data.Direction));
                        this.Logger.LogDebug($"Set Direction: {this.data.Direction}");
                        break;

                    case InverterParameterId.TableTravelDirection:
                        if (!this.data.RefreshAll)
                        {
                            this.ParentStateMachine.ChangeState(new PositioningTableEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus, this.Logger));
                        }
                        else
                        {
                            this.parameterId = 0;
                            this.tableIndex = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
                        }
                        break;

                    case InverterParameterId.TableTravelTargetSpeeds:
                        this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetAccelerations, this.data.TargetAcceleration[this.parameterId]));
                        this.Logger.LogDebug($"Set Acceleration[{this.parameterId}]: {this.data.TargetAcceleration[this.parameterId]}: dataset {this.tableIndex}");

                        break;

                    case InverterParameterId.TableTravelTargetAccelerations:
                        this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetDecelerations, this.data.TargetAcceleration[this.parameterId]));
                        this.Logger.LogDebug($"Set Deceleration[{this.parameterId}]: {this.data.TargetAcceleration[this.parameterId]}: dataset {this.tableIndex}");

                        break;

                    case InverterParameterId.TableTravelTargetDecelerations:
                        this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelSwitchPositions, this.data.SwitchPosition[this.parameterId]));
                        this.Logger.LogDebug($"Set Switch Position[{this.parameterId}]: {this.data.SwitchPosition[this.parameterId]}: dataset {this.tableIndex}");
                        break;

                    case InverterParameterId.TableTravelSwitchPositions:
                        if (++this.parameterId < this.data.SwitchPosition.Length)
                        {
                            this.tableIndex = (InverterDataset)((int)InverterDataset.TableTravelSet1 + this.parameterId);
                            this.ParentStateMachine.EnqueueMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new PositioningTableEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus, this.Logger));
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
