using System.Collections.Generic;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningSetParametersState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public PositioningSetParametersState(
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
            var definitions = new List<InverterBlockDefinition>
                    {
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.PositionTargetPosition),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.PositionTargetSpeed),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.PositionAcceleration),
                        new InverterBlockDefinition(this.InverterStatus.SystemIndex, InverterParameterId.PositionDeceleration)
                    };
            this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockDefinition, definitions));
            this.Logger.LogDebug($"Set block definition: {InverterParameterId.PositionTargetPosition}, {InverterParameterId.PositionTargetSpeed}, {InverterParameterId.PositionAcceleration}, {InverterParameterId.PositionDeceleration}. Inverter {this.InverterStatus.SystemIndex}");
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new PositioningDisableOperationState(
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
                this.ParentStateMachine.ChangeState(new PositioningErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:ID Parametro={message.ParameterId}");

                switch (message.ParameterId)
                {
                    case InverterParameterId.BlockDefinition:
                        var blockValues = new object[]
                        {
                            this.data.TargetPosition,
                            this.data.TargetSpeed[0],
                            this.data.TargetAcceleration[0],
                            this.data.TargetDeceleration[0]
                        };
                        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.BlockWrite, blockValues));
                        this.Logger.LogDebug($"Set block values: {blockValues[0]}, {blockValues[1]}, {blockValues[2]}, {blockValues[3]}. Inverter {this.InverterStatus.SystemIndex} ");
                        break;

                    //case InverterParameterId.PositionTargetPosition:
                    //    {
                    //        this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionTargetSpeed, this.data.TargetSpeed[0]));
                    //        this.Logger.LogDebug($"Set target Speed: {this.data.TargetSpeed[0]}");
                    //    }
                    //    break;

                    //case InverterParameterId.PositionTargetSpeed:
                    //    this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionAcceleration, this.data.TargetAcceleration[0]));
                    //    this.Logger.LogDebug($"Set Acceleration: {this.data.TargetAcceleration[0]}");
                    //    break;

                    //case InverterParameterId.PositionAcceleration:
                    //    this.ParentStateMachine.EnqueueCommandMessage(new InverterMessage(this.InverterStatus.SystemIndex, (short)InverterParameterId.PositionDeceleration, this.data.TargetDeceleration[0]));
                    //    this.Logger.LogDebug($"Set Deceleration: {this.data.TargetDeceleration[0]}");
                    //    break;

                    //case InverterParameterId.PositionDeceleration:
                    case InverterParameterId.BlockWrite:
                        this.ParentStateMachine.ChangeState(new PositioningEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus as IPositioningInverterStatus, this.Logger));
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
