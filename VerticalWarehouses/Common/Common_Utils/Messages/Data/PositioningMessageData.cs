﻿using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class PositioningMessageData : IPositioningMessageData
    {
        #region Constructors

        public PositioningMessageData(Axis axisMovement, MovementType movementType, decimal target, decimal speed, decimal acceleration, decimal deceleration, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = axisMovement;
            this.MovementType = movementType;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; private set; }

        public decimal CurrentPosition { get; set; }

        public MovementType MovementType { get; private set; }

        public decimal TargetAcceleration { get; private set; }

        public decimal TargetDeceleration { get; private set; }

        public decimal TargetPosition { get; private set; }

        public decimal TargetSpeed { get; private set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
