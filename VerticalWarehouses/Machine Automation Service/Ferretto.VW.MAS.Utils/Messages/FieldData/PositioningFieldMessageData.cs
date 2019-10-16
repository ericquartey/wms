﻿using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class PositioningFieldMessageData : FieldMessageData, IPositioningFieldMessageData
    {
        #region Constructors

        public PositioningFieldMessageData(
            Axis axisMovement,
            MovementType movementType,
            double target,
            double[] speed,
            double[] acceleration,
            double[] deceleration,
            int numberCycles,
            bool waitContinue,
            BayNumber requestingBay,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisMovement = axisMovement;
            this.MovementType = movementType;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.NumberCycles = numberCycles;
            this.RequestingBay = requestingBay;
            this.WaitContinue = waitContinue;
        }

        public PositioningFieldMessageData(
            IPositioningMessageData messageData,
            BayNumber requestingBay,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (messageData is null)
            {
                throw new System.ArgumentNullException(nameof(messageData));
            }

            this.AxisMovement = messageData.AxisMovement;
            this.MovementType = messageData.MovementType;
            this.TargetPosition = messageData.TargetPosition;
            this.TargetSpeed = messageData.TargetSpeed;
            this.TargetAcceleration = messageData.TargetAcceleration;
            this.TargetDeceleration = messageData.TargetDeceleration;
            this.Direction = messageData.Direction;
            this.SwitchPosition = messageData.SwitchPosition;

            this.IsTorqueCurrentSamplingEnabled = messageData.MovementMode == MovementMode.TorqueCurrentSampling;
            this.LoadedNetWeight = messageData.LoadedNetWeight;
            this.LoadingUnitId = messageData.LoadingUnitId;
            this.TorqueCurrentSample = messageData.TorqueCurrentSample;

            this.WaitContinue = messageData.WaitContinue;
            this.RequestingBay = requestingBay;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; set; }

        public HorizontalMovementDirection Direction { get; set; }

        public bool IsTorqueCurrentSamplingEnabled { get; }

        public double? LoadedNetWeight { get; set; }

        public int? LoadingUnitId { get; }

        public MovementType MovementType { get; set; }

        public int NumberCycles { get; }

        public BayNumber RequestingBay { get; set; }

        public double[] SwitchPosition { get; set; }

        public double[] TargetAcceleration { get; set; }

        public double[] TargetDeceleration { get; set; }

        public double TargetPosition { get; set; }

        public double[] TargetSpeed { get; set; }

        public DataSample TorqueCurrentSample { get; set; }

        public bool WaitContinue { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Axis:{this.AxisMovement.ToString()} Type:{this.MovementType.ToString()} NumberCycles:{this.NumberCycles} Acceleration:{this.TargetAcceleration} Deceleration:{this.TargetDeceleration} Position:{this.TargetPosition} Speed:{this.TargetSpeed}";
        }

        #endregion
    }
}
