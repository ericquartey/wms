using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IVerticalPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        decimal CurrentPosition { get; set; }

        int ExecutedCycles { get; set; }

        decimal LowerBound { get; }

        MovementType MovementType { get; }

        int NumberCycles { get; }

        decimal Resolution { get; }

        decimal TargetAcceleration { get; }

        decimal TargetDeceleration { get; }

        decimal TargetPosition { get; }

        decimal TargetSpeed { get; }

        decimal UpperBound { get; }

        #endregion
    }
}
