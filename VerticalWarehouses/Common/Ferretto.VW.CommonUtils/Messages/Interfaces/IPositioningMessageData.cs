using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        decimal CurrentPosition { get; set; }

        int ExecutedCycles { get; set; }

        bool IsOneKMachine { get; set; }

        decimal LowerBound { get; }

        MovementMode MovementMode { get; set; }

        MovementType MovementType { get; }

        int NumberCycles { get; }

        decimal TargetAcceleration { get; }

        decimal TargetDeceleration { get; }

        decimal TargetPosition { get; }

        decimal TargetSpeed { get; }

        decimal UpperBound { get; }

        #endregion
    }
}
