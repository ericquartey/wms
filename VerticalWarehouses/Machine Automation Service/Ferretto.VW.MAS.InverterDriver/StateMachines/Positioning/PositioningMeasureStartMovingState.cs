using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningMeasureStartMovingState : PositioningStartMovingState
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly ElevatorAxis verticalParams;

        private DateTime startTime;

        #endregion

        #region Constructors

        public PositioningMeasureStartMovingState(
            IInverterPositioningFieldMessageData data,
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.data = data;

            this.verticalParams = this.ParentStateMachine.GetRequiredService<IElevatorDataProvider>().GetAxis(Orientation.Vertical);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Inverter.PositionControlWord.ImmediateChangeSet = true;
            base.Start();

            this.Logger.LogInformation("Starting measure of weight.");
            this.startTime = DateTime.MinValue;
        }

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

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            base.ValidateCommandResponse(message);

            if (this.startTime == DateTime.MinValue)
            {
                this.startTime = DateTime.UtcNow;
            }
            else
            {
                if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.verticalParams.WeightMeasureTime * 100)
                {
                    this.RequestSample();
                    this.startTime = DateTime.UtcNow;
                }
            }
            if (message.ParameterId == InverterParameterId.TorqueCurrent)
            {
                this.data.MeasuredWeight = (message.UShortPayload * this.verticalParams.WeightMeasureMultiply / 10.0) + this.verticalParams.WeightMeasureSum;
                if (this.data.LoadingUnitId.HasValue)
                {
                    this.ParentStateMachine.GetRequiredService<ILoadingUnitsProvider>().SetWeight(this.data.LoadingUnitId.Value, this.data.MeasuredWeight);
                }
                this.Logger.LogInformation($"Weight measured {this.data.MeasuredWeight}. Current {message.UShortPayload / 10.0}. kMul {this.verticalParams.WeightMeasureMultiply}. kSum {this.verticalParams.WeightMeasureSum}");
                this.data.IsWeightMeasureDone = true;
                this.ParentStateMachine.ChangeState(
                    new PositioningMeasureDisableOperationState(
                        this.ParentStateMachine,
                        this.data,
                        this.Inverter,
                        this.Logger));
            }

            return true;
        }

        private void RequestSample()
        {
            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    InverterParameterId.TorqueCurrent));
        }

        #endregion
    }
}
