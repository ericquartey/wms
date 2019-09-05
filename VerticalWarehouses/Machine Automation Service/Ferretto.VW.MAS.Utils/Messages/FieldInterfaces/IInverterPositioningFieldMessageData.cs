using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisMovement { get; set; }

        int Direction { get; set; }

        MovementType MovementType { get; set; }

        int NumberCycles { get; }

        bool RefreshAll { get; }

        int[] SwitchPosition { get; set; }

        int[] TargetAcceleration { get; set; }

        int[] TargetDeceleration { get; set; }

        int TargetPosition { get; set; }

        int[] TargetSpeed { get; set; }

        #endregion
    }
}
