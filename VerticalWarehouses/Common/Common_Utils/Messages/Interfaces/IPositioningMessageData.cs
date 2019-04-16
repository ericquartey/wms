using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        decimal CurrentPosition { get; set; }

        MovementType MovementType { get; }

        decimal TargetAcceleration { get; }

        decimal TargetDeceleration { get; }

        decimal TargetPosition { get; }

        decimal TargetSpeed { get; }

        #endregion
    }
}
