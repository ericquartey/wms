using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisMovement { get; set; }

        MovementType MovementType { get; set; }

        int NumberCycles { get; }

        int TargetAcceleration { get; set; }

        int TargetDeceleration { get; set; }

        int TargetPosition { get; set; }

        int TargetSpeed { get; set; }

        #endregion
    }
}
