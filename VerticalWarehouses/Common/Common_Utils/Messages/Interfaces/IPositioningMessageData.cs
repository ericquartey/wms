using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        decimal CurrentPosition { get; set; }

        decimal LowerBound { get; }

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
