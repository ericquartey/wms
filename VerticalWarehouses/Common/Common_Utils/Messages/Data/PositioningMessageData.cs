using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class PositioningMessageData : IPositioningMessageData
    {
        #region Constructors

        public PositioningMessageData(Axis axisMovement, PositioningType positioningType, decimal target, decimal speed, decimal acceleration, decimal deceleration, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisMovement = axisMovement;
            this.Type = positioningType;
            this.TargetPosition = target;
            this.TargetSpeed = speed;
            this.TargetAcceleration = acceleration;
            this.TargetDeceleration = deceleration;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisMovement { get; private set; }

        public decimal TargetAcceleration { get; private set; }

        public decimal TargetDeceleration { get; private set; }

        public decimal TargetPosition { get; private set; }

        public decimal TargetSpeed { get; private set; }

        public PositioningType Type { get; private set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
