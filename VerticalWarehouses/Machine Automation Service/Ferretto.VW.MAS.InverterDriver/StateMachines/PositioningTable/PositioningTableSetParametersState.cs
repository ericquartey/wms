using System;
using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    /// <summary>
    /// Sequence of transmission of parameters:
    /// 1) Set table index: 33 (par. 1200)
    /// 2) Set target position: TotalDistance (par. 1202) (*)
    /// 3) Set Direction: (par 1261)
    /// 4) Set table index: 50 (par. 1200) (*)
    /// 5) Set target Speed[0]: (par. 1203) (*)
    /// 6) Set Acceleration[0]: (par. 1204) (*)
    /// 7) Set Deceleration[0]: (par. 1206) (*)
    /// 8) Set table index: 51 (*)
    /// 9) Set target Speed[1]: (par. 1203) (*)
    /// 10)Set Acceleration[1]: (par. 1204) (*)
    /// 11)Set Deceleration[1]: (par. 1206) (*)
    /// 12)Set table index: 52 (*)
    /// 13)Set target Speed[2]: (par. 1203) (*)
    /// 14)Set Acceleration[2]: (par. 1204) (*)
    /// 15)Set Deceleration[2]: (par. 1206) (*)
    /// 16)Set table index: 53 (*)
    /// 17)Set target Speed[3]: (par. 1203) (*)
    /// 18)Set Acceleration[3]: (par. 1204) (*)
    /// 19)Set Deceleration[3]: (par. 1206) (*)
    /// 20)Set table index: 54 (*)
    /// 30)Set target Speed[4]: (par. 1203) (*)
    /// 31)Set Acceleration[4]: (par. 1204) (*)
    /// 32)Set Deceleration[4]: (par. 1206) (*)
    /// 33)Set table index: 56 (*)
    /// 34)Set Switch Position[0]: (par. 1202) (*)
    /// 35)Set table index: 57 (*)
    /// 36)Set Switch Position[1]: (par. 1202) (*)
    /// 37)Set table index: 58 (*)
    /// 38)Set Switch Position[2]: (par. 1202) (*)
    /// 39)Set table index: 59 (*)
    /// 40)Set Switch Position[3]: (par. 1202) (*)
    ///
    /// (*) parameters are transmitted only when changed
    /// </summary>
    internal class PositioningTableSetParametersState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly IInverterPositioningFieldMessageData dataOld;

        private bool isBlockDefined;

        private bool isPositionDefined;

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
            this.stepId = 0;
            this.isBlockDefined = false;
            var maxSpeed = 30000;   // just a not zero minimum value
            if (this.data.TargetSpeed.Length > 2)
            {
                for (var i = 1; i < this.data.TargetSpeed.Length - 1; i++)
                {
                    // ignore first and last step
                    maxSpeed = Math.Max(maxSpeed, this.data.TargetSpeed[i]);
                }
            }
            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeed, maxSpeed));
            this.Logger.LogDebug($"Set PositionTargetSpeed for emergency stop: {maxSpeed}");
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
                    case InverterParameterId.PositionTargetSpeed:
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.TableTravelTableIndex, (short)this.tableIndex));
                        this.Logger.LogDebug($"Set table index: {this.tableIndex}");
                        break;

                    case InverterParameterId.BlockDefinition:
                        if (!this.isPositionDefined)
                        {
                            this.isBlockDefined = true;
                            var blockValues = new object[]
                            {
                                (short)this.tableIndex,
                                this.data.TargetSpeed[this.stepId],
                                this.data.TargetAcceleration[this.stepId],
                                this.data.TargetDeceleration[this.stepId]
                            };
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockWrite, blockValues));
                            this.Logger.LogDebug($"Set block values: {blockValues[0]}, {blockValues[1]}, {blockValues[2]}, {blockValues[3]} ");
                        }
                        else
                        {
                            var blockValues = new object[]
                            {
                                (short)InverterTableIndex.TableTravelP1,
                                this.data.SwitchPosition[0],
                                (short)InverterTableIndex.TableTravelP2,
                                this.data.SwitchPosition[1],
                                (short)InverterTableIndex.TableTravelP3,
                                this.data.SwitchPosition[2],
                                (short)InverterTableIndex.TableTravelP4,
                                this.data.SwitchPosition[3],
                            };
                            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockWrite, blockValues));
                            this.Logger.LogDebug($"Set block values: {blockValues[0]}, {blockValues[1]}, {blockValues[2]}, {blockValues[3]}, {blockValues[4]}, {blockValues[5]}, {blockValues[6]}, {blockValues[7]} ");
                        }
                        break;

                    case InverterParameterId.BlockWrite:
                        if (!this.isPositionDefined)
                        {
                            this.DoTargetDecelerations();
                        }
                        else
                        {
                            this.stepId = this.data.SwitchPosition.Length;
                            this.DoTargetPosition();
                        }
                        break;

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
                if (!this.isBlockDefined)
                {
                    var definitions = new List<InverterBlockDefinition>
                    {
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetSpeeds),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetAccelerations),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetDecelerations)
                    };
                    this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockDefinition, definitions));
                    this.Logger.LogDebug($"Set block definition: {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetSpeeds}, {InverterParameterId.TableTravelTargetAccelerations}, {InverterParameterId.TableTravelTargetDecelerations}");
                }
                else
                {
                    var blockValues = new object[]
                    {
                        (short)this.tableIndex,
                        this.data.TargetSpeed[this.stepId],
                        this.data.TargetAcceleration[this.stepId],
                        this.data.TargetDeceleration[this.stepId]
                    };
                    this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockWrite, blockValues));
                    this.Logger.LogDebug($"Set block values: {blockValues[0]}, {blockValues[1]}, {blockValues[2]}, {blockValues[3]} ");
                }
            }
            else
            {
                this.stepId = 0;
                this.tableIndex = (InverterTableIndex)((short)InverterTableIndex.TableTravelP1 + this.stepId);
                var definitions = new List<InverterBlockDefinition>
                    {
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                    };
                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockDefinition, definitions));
                this.Logger.LogDebug($"Set block definition: {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}, {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}, {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}, {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}");
                this.isPositionDefined = true;
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
                var definitions = new List<InverterBlockDefinition>
                    {
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTableIndex),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.TableTravelTargetPosition),
                    };
                this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockDefinition, definitions));
                this.Logger.LogDebug($"Set block definition: {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}, {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}, {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}, {InverterParameterId.TableTravelTableIndex}, {InverterParameterId.TableTravelTargetPosition}");
                this.isPositionDefined = true;
            }
            else
            {
                this.ParentStateMachine.ChangeState(new PositioningTableEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus as IPositioningInverterStatus, this.Logger));
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
                for (var iStep = step; iStep < this.data.SwitchPosition.Length; iStep++)
                {
                    if (this.data.TargetSpeed[iStep] != (this.dataOld?.TargetSpeed[iStep] ?? 0) ||
                        this.data.TargetAcceleration[iStep] != (this.dataOld?.TargetAcceleration[iStep] ?? 0) ||
                        this.data.TargetDeceleration[iStep] != (this.dataOld?.TargetDeceleration[iStep] ?? 0)
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
