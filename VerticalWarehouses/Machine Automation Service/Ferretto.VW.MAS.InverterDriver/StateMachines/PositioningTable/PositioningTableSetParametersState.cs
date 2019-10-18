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

        private readonly IInverterPositioningFieldMessageData dataOld;

        private int stepId;

        private InverterTableIndex tableIndex;

        #endregion

        #region Constructors

        public PositioningTableSetParametersState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IInverterPositioningFieldMessageData dataOld,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;

            if (this.data.RefreshAll)
            {
                this.dataOld = null;
            }
            else
            {
                this.dataOld = dataOld;
            }

            logger.LogDebug("1:Method Start");
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.tableIndex = InverterTableIndex.TableTravelP7;
            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
            this.Logger.LogDebug($"Set table index: {this.tableIndex}");
            this.stepId = 0;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new PositioningTableDisableOperationState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger,
                    true));
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
                                if (this.data.TargetPosition != (this.dataOld?.TargetPosition ?? 0))
                                {
                                    this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetPosition, this.data.TargetPosition));
                                    this.Logger.LogDebug($"Set target position: {this.data.TargetPosition}");
                                }
                                else
                                {
                                    this.DoTargetPosition();
                                }
                                break;

                            case InverterTableIndex.TableTravelSet1:
                            case InverterTableIndex.TableTravelSet2:
                            case InverterTableIndex.TableTravelSet3:
                            case InverterTableIndex.TableTravelSet4:
                            case InverterTableIndex.TableTravelSet5:
                                if (this.data.TargetSpeed[this.stepId] != (this.dataOld?.TargetSpeed[this.stepId] ?? 0))
                                {
                                    this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetSpeeds, this.data.TargetSpeed[this.stepId]));
                                    this.Logger.LogDebug($"Set target Speed[{this.stepId}]: {this.data.TargetSpeed[this.stepId]}: table index {this.tableIndex}");
                                }
                                else
                                {
                                    this.DoTargetSpeeds();
                                }
                                break;

                            case InverterTableIndex.TableTravelP1:
                            case InverterTableIndex.TableTravelP2:
                            case InverterTableIndex.TableTravelP3:
                            case InverterTableIndex.TableTravelP4:
                                if (this.data.SwitchPosition[this.stepId] != (this.dataOld?.SwitchPosition[this.stepId] ?? 0))
                                {
                                    this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetPosition, this.data.SwitchPosition[this.stepId]));
                                    this.Logger.LogDebug($"Set Switch Position[{this.stepId}]: {this.data.SwitchPosition[this.stepId]}: table index {this.tableIndex}");
                                }
                                else
                                {
                                    this.DoTargetPosition();
                                }
                                break;
                        }
                        break;

                    case InverterParameterId.TableTravelTargetPosition:
                        this.DoTargetPosition();
                        break;

                    case InverterParameterId.TableTravelDirection:
                        this.stepId = -1;
                        this.DoTargetDecelerations();
                        break;

                    case InverterParameterId.TableTravelTargetSpeeds:
                        this.DoTargetSpeeds();
                        break;

                    case InverterParameterId.TableTravelTargetAccelerations:
                        this.DoTargetAccelerations();
                        break;

                    case InverterParameterId.TableTravelTargetDecelerations:
                        this.DoTargetDecelerations();
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

        private void DoTargetAccelerations()
        {
            if (this.data.TargetAcceleration[this.stepId] != (this.dataOld?.TargetAcceleration[this.stepId] ?? 0))
            {
                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetDecelerations, this.data.TargetAcceleration[this.stepId]));
                this.Logger.LogDebug($"Set Deceleration[{this.stepId}]: {this.data.TargetAcceleration[this.stepId]}: table index {this.tableIndex}");
            }
            else
            {
                this.DoTargetDecelerations();
            }
        }

        private void DoTargetDecelerations()
        {
            this.stepId = this.FindChangedStep(this.stepId);
            if (this.stepId < this.data.SwitchPosition.Length)
            {
                this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelSet1 + this.stepId);
                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                this.Logger.LogDebug($"Set table index: {this.tableIndex}");
            }
            else
            {
                this.stepId = 0;
                this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelP1 + this.stepId);
                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                this.Logger.LogDebug($"Set table index: {this.tableIndex}");
            }
        }

        private void DoTargetPosition()
        {
            if (this.tableIndex == InverterTableIndex.TableTravelP7)
            {
                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelDirection, (short)this.data.Direction));
                this.Logger.LogDebug($"Set Direction: {this.data.Direction}");
            }
            else if (++this.stepId < this.data.SwitchPosition.Length - 1)
            {
                this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelP1 + this.stepId);
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
        }

        private void DoTargetSpeeds()
        {
            if (this.data.TargetAcceleration[this.stepId] != (this.dataOld?.TargetAcceleration[this.stepId] ?? 0))
            {
                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTargetAccelerations, this.data.TargetAcceleration[this.stepId]));
                this.Logger.LogDebug($"Set Acceleration[{this.stepId}]: {this.data.TargetAcceleration[this.stepId]}: table index {this.tableIndex}");
            }
            else
            {
                this.DoTargetAccelerations();
            }
        }

        private int FindChangedStep(int step)
        {
            while (++step < this.data.SwitchPosition.Length)
            {
                for (int iStep = step; iStep < this.data.SwitchPosition.Length; iStep++)
                {
                    if (this.data.TargetSpeed[iStep] != (this.dataOld?.TargetSpeed[iStep] ?? 0) ||
                        this.data.TargetAcceleration[iStep] != (this.dataOld?.TargetAcceleration[iStep] ?? 0)
                        )
                    {
                        return step;
                    }
                }
            }
            return step;
        }

        #endregion
    }
}
