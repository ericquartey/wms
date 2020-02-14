﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class ShutterPositioningFieldMessageData : FieldMessageData, IShutterPositioningFieldMessageData
    {
        #region Constructors

        public ShutterPositioningFieldMessageData(
            ShutterPosition shutterPosition,
            ShutterMovementDirection shutterMovementDirection,
            ShutterType shutterType,
            double speedRate,
            double highSpeedDurationOpen,
            double highSpeedDurationClose,
            double lowerSpeed,
            MovementType movementType,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.ShutterPosition = shutterPosition;
            this.ShutterMovementDirection = shutterMovementDirection;
            this.ShutterType = shutterType;
            this.SpeedRate = speedRate;
            this.HighSpeedDurationOpen = highSpeedDurationOpen;
            this.HighSpeedDurationClose = highSpeedDurationClose;
            this.LowerSpeed = lowerSpeed;
            this.MovementType = movementType;
        }

        public ShutterPositioningFieldMessageData(
            IShutterPositioningMessageData messageData,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            if (messageData == null)
            {
                throw new System.ArgumentNullException(nameof(messageData));
            }

            this.ShutterPosition = messageData.ShutterPosition;
            this.ShutterMovementDirection = messageData.ShutterMovementDirection;
            this.ShutterType = messageData.ShutterType;
            this.SpeedRate = messageData.SpeedRate;
            this.HighSpeedDurationOpen = messageData.HighSpeedDurationOpen;
            this.HighSpeedDurationClose = messageData.HighSpeedDurationClose;
            this.LowerSpeed = messageData.LowerSpeed;
            this.MovementType = messageData.MovementType;
        }

        #endregion

        #region Properties

        public double HighSpeedDurationClose { get; }

        public double HighSpeedDurationOpen { get; }

        public double LowerSpeed { get; }

        public MovementType MovementType { get; }

        public ShutterMovementDirection ShutterMovementDirection { get; }

        public ShutterPosition ShutterPosition { get; set; }

        public ShutterType ShutterType { get; }

        public double SpeedRate { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Direction:{this.ShutterMovementDirection.ToString()} Position:{this.ShutterPosition.ToString()} Type:{this.ShutterType.ToString()} SpeedRate:{this.SpeedRate}";
        }

        #endregion
    }
}
