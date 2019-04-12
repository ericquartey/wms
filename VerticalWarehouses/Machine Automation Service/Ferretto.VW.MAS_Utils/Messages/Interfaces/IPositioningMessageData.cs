using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        MovementType MovementType { get; }

        decimal TargetAcceleration { get; }

        decimal TargetDeceleration { get; }

        decimal TargetPosition { get; }

        decimal TargetSpeed { get; }

        #endregion
    }
}
