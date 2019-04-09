using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        decimal TargetAcceleration { get; }

        decimal TargetDeceleration { get; }

        decimal TargetPosition { get; }

        decimal TargetSpeed { get; }

        MovementType TypeOfMovement { get; }

        #endregion
    }
}
